using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration _configuration;

    public ApplicationFeeAppService(
        IRepository<ApplicationFee, Guid> feeRepository,
        IRepository<Application, Guid> applicationRepository,
        IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> settingsRepository,
        WorkflowStarterService workflowStarter,
        IConfiguration configuration)
    {
        _feeRepository = feeRepository;
        _applicationRepository = applicationRepository;
        _settingsRepository = settingsRepository;
        _workflowStarter = workflowStarter;
        _configuration = configuration;
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

    /// <summary>
    /// Which payment gateway this deployment is wired to. There is no real one
    /// yet; see <see cref="PaymentGatewayMode"/>.
    /// </summary>
    public PaymentGatewayMode GatewayMode =>
        Enum.TryParse<PaymentGatewayMode>(_configuration["Admissions:PaymentGateway"], ignoreCase: true, out var mode)
            ? mode
            : PaymentGatewayMode.Simulated;

    /// <summary>
    /// What the applicant needs in order to deal with the fee: how much, what
    /// state it is in, and whether there is anything online to click.
    /// </summary>
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    public async Task<ApplicationFeeCheckoutDto> GetCheckoutAsync(Guid applicationId)
    {
        var application = await _applicationRepository.GetAsync(applicationId);
        var settings = await GetAdmissionSettingsAsync(application.AcademicYearId, application.AppliedGradeId);
        var fee = await _feeRepository.FirstOrDefaultAsync(f => f.ApplicationId == applicationId);

        return new ApplicationFeeCheckoutDto
        {
            ApplicationId = applicationId,
            ApplicationNumber = application.ApplicationNumber,
            FeeRequired = settings.RequiresApplicationFee(),
            Amount = settings.ApplicationFeeAmount,
            Currency = fee?.Currency ?? "ZAR",
            Status = fee?.Status ?? PaymentStatus.Pending,
            PaymentReference = fee?.PaymentReference,
            ReceiptNumber = fee?.ReceiptNumber,
            PaymentDate = fee?.PaymentDate,
            AwaitingPayment = application.Status == ApplicationStatus.PaymentPending,
            GatewayMode = GatewayMode,
            IsSimulated = GatewayMode == PaymentGatewayMode.Simulated,
        };
    }

    /// <summary>
    /// Settles the fee through the stand-in gateway, so the admissions journey
    /// can be walked from end to end before a real one is chosen.
    /// <para>
    /// <b>No money moves.</b> It is refused outright unless this deployment is
    /// configured for the simulated gateway, and everything it writes says so —
    /// the payment method, the reference and the receipt number all carry
    /// SIMULATED, so a row in the finance export can never be mistaken for a
    /// payment the school actually received.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Admissions_Applications_Submit)]
    public async Task<PaymentResultDto> SimulatePaymentAsync(Guid applicationId)
    {
        if (GatewayMode != PaymentGatewayMode.Simulated)
            throw new UserFriendlyException(AdmissionsExceptionCodes.PaymentGatewayNotAvailable,
                "Online payment is not available for this school. Pay the application fee by EFT "
                + "or at the school office, and they will record it against your application.");

        var application = await _applicationRepository.GetAsync(applicationId);

        if (application.Status != ApplicationStatus.PaymentPending)
            throw new UserFriendlyException(AdmissionsExceptionCodes.FeeAlreadyPaid,
                "This application is not waiting for payment.");

        var reference = $"SIMULATED-{DateTime.UtcNow:yyyyMMddHHmmss}";

        return await SettleAsync(application, new RecordPaymentDto
        {
            PaymentMethod = SouthAfricanPaymentMethod.CreditCard,
            PaymentReference = reference,
            ReceiptNumber = $"SIMULATED-{GenerateReceiptNumber()}",
        });
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_RecordManual)]
    public async Task<PaymentResultDto> RecordPaymentAsync(Guid applicationId, RecordPaymentDto input)
    {
        var application = await _applicationRepository.GetAsync(applicationId);

        if (application.Status != ApplicationStatus.PaymentPending)
            throw new UserFriendlyException(AdmissionsExceptionCodes.FeeAlreadyPaid,
                "Application is not in payment pending status.");

        return await SettleAsync(application, input);
    }

    /// <summary>
    /// Marks the fee settled and moves the application into review.
    /// <para>
    /// Shared by the office recording a payment it received and by the stand-in
    /// gateway, because what happens to the application afterwards is the same
    /// either way and should not be written twice.
    /// </para>
    /// </summary>
    private async Task<PaymentResultDto> SettleAsync(Application application, RecordPaymentDto input)
    {
        var fee = await _feeRepository.FirstOrDefaultAsync(f => f.ApplicationId == application.Id);
        var isNewFee = fee == null;

        if (isNewFee)
        {
            var settings = await GetAdmissionSettingsAsync(application.AcademicYearId, application.AppliedGradeId);
            fee = new ApplicationFee(
                Guid.NewGuid(),
                application.Id,
                settings.ApplicationFeeAmount,
                "ZAR") { TenantId = AbpSession.TenantId };
        }

        fee.Status = PaymentStatus.Completed;
        fee.PaymentMethod = input.PaymentMethod;
        fee.PaymentReference = input.PaymentReference;
        fee.PaymentDate = DateTime.UtcNow;
        fee.ReceiptNumber = input.ReceiptNumber ?? GenerateReceiptNumber();

        if (isNewFee)
            await _feeRepository.InsertAsync(fee);
        else
            await _feeRepository.UpdateAsync(fee);

        // Backfill TenantId if NULL (pre-fix records)
        if (application.TenantId == null) application.TenantId = AbpSession.TenantId;
        application.MarkPaymentReceived();
        await _applicationRepository.UpdateAsync(application);

        await CurrentUnitOfWork.SaveChangesAsync();

        // Review is what the admissions workflow is for. Never allowed to fail
        // the payment: money that was received stays received.
        try
        {
            await _workflowStarter.TryStartWorkflowAsync(
                AbpSession.TenantId,
                WorkflowEntityType.Application,
                application.Id,
                AbpSession.UserId ?? 0,
                (AbpSession.UserId ?? 0).ToString());
        }
        catch (Exception ex)
        {
            Logger.Warn($"Could not auto-start workflow for application {application.Id}: {ex.Message}");
        }

        return new PaymentResultDto
        {
            Success = true,
            Message = "Payment recorded successfully.",
            ApplicationId = application.Id,
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
