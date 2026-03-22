using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.ApplicationFees.Dto;
using psms.Admissions.Shared;
using psms.Authorization;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Shared;
using System;
using System.Threading.Tasks;

namespace psms.Admissions.ApplicationFees;

/// <summary>
/// Service for managing application fees and payments.
/// Implements ADM-006, ADM-007.
/// </summary>
[AbpAuthorize(PermissionNames.Admissions_Applications)]
public class ApplicationFeeAppService : ApplicationService, IApplicationFeeAppService
{
    private readonly IRepository<ApplicationFee, Guid> _feeRepository;
    private readonly IRepository<Application, Guid> _applicationRepository;
    private readonly IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> _settingsRepository;
    private readonly WorkflowStarterService _workflowStarter;

    public ApplicationFeeAppService(
        IRepository<ApplicationFee, Guid> feeRepository,
        IRepository<Application, Guid> applicationRepository,
        IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> settingsRepository,
        WorkflowStarterService workflowStarter)
    {
        _feeRepository = feeRepository;
        _applicationRepository = applicationRepository;
        _settingsRepository = settingsRepository;
        _workflowStarter = workflowStarter;
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    public async Task<ApplicationFeeDto> GetByApplicationAsync(Guid applicationId)
    {
        var fee = await _feeRepository
            .GetAll()
            .Include(f => f.Application)
            .FirstOrDefaultAsync(f => f.ApplicationId == applicationId && f.TenantId == AbpSession.TenantId);

        if (fee == null)
            return null;

        var dto = ObjectMapper.Map<ApplicationFeeDto>(fee);
        dto.ApplicationNumber = fee.Application?.ApplicationNumber;
        return dto;
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_RecordManual)]
    public async Task<PaymentResultDto> RecordPaymentAsync(Guid applicationId, RecordPaymentDto input)
    {
        var application = await _applicationRepository.GetAsync(applicationId);

        if (application.Status != ApplicationStatus.PaymentPending)
            throw new UserFriendlyException(AdmissionsExceptionCodes.FeeAlreadyPaid,
                "Application is not in payment pending status.");

        var fee = await _feeRepository.FirstOrDefaultAsync(f => f.ApplicationId == applicationId);
        var isNewFee = fee == null;

        if (isNewFee)
        {
            // Create fee record
            var settings = await GetAdmissionSettingsAsync(application.AcademicYearId, application.AppliedGradeId);
            fee = new ApplicationFee(
                Guid.NewGuid(),
                applicationId,
                settings.ApplicationFeeAmount,
                "ZAR") { TenantId = AbpSession.TenantId };
        }

        // Record payment
        fee.Status = PaymentStatus.Completed;
        fee.PaymentMethod = input.PaymentMethod;
        fee.PaymentReference = input.PaymentReference;
        fee.PaymentDate = DateTime.UtcNow;
        fee.ReceiptNumber = input.ReceiptNumber ?? GenerateReceiptNumber();

        if (isNewFee)
            await _feeRepository.InsertAsync(fee);
        else
            await _feeRepository.UpdateAsync(fee);

        // Update application status: PaymentPending → UnderReview
        // Backfill TenantId if NULL (pre-fix records)
        if (application.TenantId == null) application.TenantId = AbpSession.TenantId;
        application.MarkPaymentReceived();
        await _applicationRepository.UpdateAsync(application);

        await CurrentUnitOfWork.SaveChangesAsync();

        // Auto-start admissions workflow (silently skips if no definition configured)
        try
        {
            await _workflowStarter.TryStartWorkflowAsync(
                AbpSession.TenantId,
                WorkflowEntityType.Application,
                applicationId,
                AbpSession.UserId.Value,
                AbpSession.UserId.Value.ToString());
        }
        catch (Exception ex)
        {
            Logger.Warn($"Could not auto-start workflow for application {applicationId}: {ex.Message}");
        }

        return new PaymentResultDto
        {
            Success = true,
            Message = "Payment recorded successfully.",
            ApplicationId = applicationId,
            FeeId = fee.Id,
            PaymentReference = fee.PaymentReference,
            Status = fee.Status,
            Amount = fee.Amount,
            Currency = fee.Currency,
            ReceiptNumber = fee.ReceiptNumber
        };
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_RecordManual)]
    public async Task<PaymentResultDto> ProcessPaymentCallbackAsync(PaymentCallbackDto input)
    {
        var fee = await _feeRepository.FirstOrDefaultAsync(f => f.PaymentReference == input.MerchantReference);

        if (fee == null)
            return new PaymentResultDto
            {
                Success = false,
                Message = "Payment reference not found.",
                ErrorCode = "REFERENCE_NOT_FOUND"
            };

        var application = await _applicationRepository.GetAsync(fee.ApplicationId);

        // Check if payment was successful based on status
        var isSuccess = input.Status?.ToLower() == "complete" || input.Status?.ToLower() == "completed";

        if (isSuccess)
        {
            fee.Status = PaymentStatus.Completed;
            fee.PaymentDate = DateTime.UtcNow;
            fee.ReceiptNumber = GenerateReceiptNumber();

            // Update application status: PaymentPending → UnderReview
            if (application.Status == ApplicationStatus.PaymentPending)
            {
                application.MarkPaymentReceived();
                await _applicationRepository.UpdateAsync(application);
            }
        }
        else
        {
            fee.Status = PaymentStatus.Failed;
        }

        await _feeRepository.UpdateAsync(fee);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Auto-start admissions workflow on successful payment
        if (isSuccess && application.Status == ApplicationStatus.UnderReview)
        {
            await _workflowStarter.TryStartWorkflowAsync(
                application.TenantId,
                WorkflowEntityType.Application,
                application.Id,
                AbpSession.UserId ?? 0,
                "System");
        }

        return new PaymentResultDto
        {
            Success = isSuccess,
            Message = isSuccess ? "Payment successful." : "Payment failed.",
            ApplicationId = fee.ApplicationId,
            FeeId = fee.Id,
            PaymentReference = fee.PaymentReference,
            Status = fee.Status,
            Amount = fee.Amount,
            Currency = fee.Currency,
            ReceiptNumber = fee.ReceiptNumber,
            ErrorCode = !isSuccess ? input.Status : null,
            ErrorDetails = input.ErrorMessage
        };
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    public async Task<PaymentResultDto> GetPaymentStatusAsync(Guid applicationId)
    {
        var fee = await _feeRepository.FirstOrDefaultAsync(f => f.ApplicationId == applicationId);

        if (fee == null)
        {
            return new PaymentResultDto
            {
                Success = false,
                Message = "No payment record found.",
                ApplicationId = applicationId,
                Status = PaymentStatus.Pending
            };
        }

        return new PaymentResultDto
        {
            Success = fee.Status == PaymentStatus.Completed,
            Message = fee.Status == PaymentStatus.Completed ? "Payment completed." : "Payment pending.",
            ApplicationId = applicationId,
            FeeId = fee.Id,
            PaymentReference = fee.PaymentReference,
            Status = fee.Status,
            Amount = fee.Amount,
            Currency = fee.Currency,
            ReceiptNumber = fee.ReceiptNumber
        };
    }

    #region Private Methods

    private async Task<Domain.Admissions.Entities.AdmissionSettings> GetAdmissionSettingsAsync(Guid academicYearId, Guid gradeId)
    {
        var settings = await _settingsRepository
            .GetAll().AsNoTracking()
            .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == gradeId);

        if (settings == null)
        {
            settings = await _settingsRepository
                .GetAll().AsNoTracking()
                .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == null);
        }

        if (settings == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.AdmissionSettingsNotFound,
                "Admission settings not found.");

        return settings;
    }

    private string GenerateReceiptNumber()
    {
        var tenantId = AbpSession.TenantId ?? 0;
        return $"RCP-{tenantId:D3}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }

    #endregion
}
