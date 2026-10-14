using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Parents;
using psms.Academic.Students;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.Shared.Enums;
using psms.Financial.Payments.Dto;
using psms.Financial.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Financial.Payments;

/// <summary>
/// Service for managing payments.
/// </summary>
[AbpAuthorize(PermissionNames.Financial_Payments)]
public class PaymentAppService : ApplicationService, IPaymentAppService
{
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<Parent, Guid> _parentRepository;
    private readonly ICurrentStudentResolver _currentStudent;
    private readonly ICurrentParentResolver _currentParent;

    public PaymentAppService(
        IRepository<Payment, Guid> paymentRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<Parent, Guid> parentRepository,
        ICurrentStudentResolver currentStudent,
        ICurrentParentResolver currentParent)
    {
        _paymentRepository = paymentRepository;
        _studentRepository = studentRepository;
        _parentRepository = parentRepository;
        _currentStudent = currentStudent;
        _currentParent = currentParent;
    }

    /// <summary>
    /// The learners a family caller may see payments for: a student themselves,
    /// a parent their linked children. Null for staff, who are not scoped.
    /// <para>
    /// Parents hold <c>Financial.Payments.View</c> for their own payments, and
    /// nothing here checked whose payment it was — any payment in the school
    /// could be read by id, by learner, by parent, or by walking the sequential
    /// receipt numbers (MOB-BE-04 follow-up).
    /// </para>
    /// </summary>
    private async Task<List<Guid>> FamilyScopeAsync()
    {
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue) return new List<Guid> { selfId.Value };

        return await _currentParent.GetCurrentChildStudentIdsAsync();
    }

    /// <summary>
    /// Out of scope reads the same as missing, so the response does not confirm
    /// that the payment exists.
    /// </summary>
    private async Task AssertInFamilyScopeAsync(Payment payment)
    {
        var scope = await FamilyScopeAsync();
        if (scope != null && !scope.Contains(payment.StudentId))
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentNotFound,
                "Payment not found.");
    }

    /// <summary>
    /// Settling a payment is the school's or the gateway's word that the money
    /// arrived. Parents hold <c>Financial.Payments.Process</c>, and without this
    /// a parent could mark their own pending EFT as paid without paying it.
    /// </summary>
    private async Task AssertNotFamilyCallerAsync()
    {
        if (await FamilyScopeAsync() != null)
            throw new AbpAuthorizationException("Only the school can settle a payment.");
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_View)]
    public async Task<PaymentDto> GetAsync(Guid id)
    {
        var payment = await _paymentRepository
            .GetAll()
            .Include(p => p.Student)
            .Include(p => p.Parent)
            .Include(p => p.PaymentAllocations)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == AbpSession.TenantId);

        if (payment == null)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentNotFound,
                "Payment not found.");

        await AssertInFamilyScopeAsync(payment);

        return ObjectMapper.Map<PaymentDto>(payment);
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_ViewAll)]
    public async Task<PagedResultDto<PaymentListDto>> GetAllAsync(GetPaymentsInput input)
    {
        var scope = await FamilyScopeAsync();

        var query = _paymentRepository
            .GetAll()
            .Include(p => p.Student)
            .Include(p => p.Parent)
            .Include(p => p.PaymentAllocations)
            .Where(p => p.TenantId == AbpSession.TenantId)
            .WhereIf(scope != null, p => scope.Contains(p.StudentId))
            .WhereIf(input.StudentId.HasValue, p => p.StudentId == input.StudentId.Value)
            .WhereIf(input.ParentId.HasValue, p => p.ParentId == input.ParentId.Value)
            .WhereIf(input.Status.HasValue, p => p.Status == input.Status.Value)
            .WhereIf(input.PaymentMethod.HasValue, p => p.PaymentMethod == input.PaymentMethod.Value)
            .WhereIf(input.FromDate.HasValue, p => p.PaymentDate >= input.FromDate.Value)
            .WhereIf(input.ToDate.HasValue, p => p.PaymentDate <= input.ToDate.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ReceiptNumber),
                p => p.ReceiptNumber == input.ReceiptNumber.Trim())
            .WhereIf(!string.IsNullOrWhiteSpace(input.StudentName),
                p => (p.Student.FirstName + " " + p.Student.LastName).ToLower()
                    .Contains(input.StudentName.Trim().ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                p => p.ReceiptNumber.ToLower().Contains(input.Keyword.Trim().ToLower())
                    || p.Student.FirstName.ToLower().Contains(input.Keyword.Trim().ToLower())
                    || p.Student.LastName.ToLower().Contains(input.Keyword.Trim().ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "PaymentDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<PaymentListDto>(
            totalCount,
            ObjectMapper.Map<List<PaymentListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_View)]
    public async Task<ListResultDto<PaymentListDto>> GetByStudentAsync(Guid studentId)
    {
        var scope = await FamilyScopeAsync();

        var items = await _paymentRepository
            .GetAll()
            .Include(p => p.Student)
            .Include(p => p.Parent)
            .Include(p => p.PaymentAllocations)
            .Where(p => p.TenantId == AbpSession.TenantId && p.StudentId == studentId)
            .WhereIf(scope != null, p => scope.Contains(p.StudentId))
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return new ListResultDto<PaymentListDto>(
            ObjectMapper.Map<List<PaymentListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_View)]
    public async Task<ListResultDto<PaymentListDto>> GetByParentAsync(Guid parentId)
    {
        var scope = await FamilyScopeAsync();

        var items = await _paymentRepository
            .GetAll()
            .Include(p => p.Student)
            .Include(p => p.Parent)
            .Include(p => p.PaymentAllocations)
            .Where(p => p.TenantId == AbpSession.TenantId && p.ParentId == parentId)
            .WhereIf(scope != null, p => scope.Contains(p.StudentId))
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return new ListResultDto<PaymentListDto>(
            ObjectMapper.Map<List<PaymentListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_RecordManual)]
    public async Task<PaymentDto> CreateAsync(CreatePaymentDto input)
    {
        // Validate Student exists
        var student = await _studentRepository
            .FirstOrDefaultAsync(s => s.Id == input.StudentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(FinancialExceptionCodes.StudentNotFound,
                "Student not found.");

        // Validate Parent exists
        var parent = await _parentRepository
            .FirstOrDefaultAsync(p => p.Id == input.ParentId && p.TenantId == AbpSession.TenantId);

        if (parent == null)
            throw new UserFriendlyException(FinancialExceptionCodes.ParentNotFound,
                "Parent not found.");

        // Validate PaymentDate not in the future
        if (input.PaymentDate.HasValue && input.PaymentDate.Value > DateTime.UtcNow)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentDateInFuture,
                "Payment date cannot be in the future.");

        // Generate receipt number: REC-{nnnnnn}
        var receiptNumber = await GenerateReceiptNumberAsync();

        var payment = new Payment(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.StudentId,
            input.ParentId,
            input.Amount,
            input.PaymentMethod,
            receiptNumber)
        {
            PaymentDate = input.PaymentDate ?? DateTime.UtcNow,
            PaymentReference = input.PaymentReference,
            Notes = input.Notes
        };

        await _paymentRepository.InsertAsync(payment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(payment.Id);
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_RecordManual)]
    public async Task<PaymentDto> UpdateAsync(Guid id, UpdatePaymentDto input)
    {
        var payment = await _paymentRepository
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == AbpSession.TenantId);

        if (payment == null)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentNotFound,
                "Payment not found.");

        if (payment.Status == PaymentStatus.Completed || payment.Status == PaymentStatus.Refunded
            || payment.Status == PaymentStatus.Cancelled || payment.Status == PaymentStatus.Failed)
            throw new UserFriendlyException(FinancialExceptionCodes.CannotUpdatePayment,
                "Cannot update a payment that is completed, refunded, cancelled, or failed.");

        if (input.PaymentReference != null) payment.PaymentReference = input.PaymentReference;
        if (input.Notes != null) payment.Notes = input.Notes;
        if (input.PaymentMethod.HasValue) payment.PaymentMethod = input.PaymentMethod.Value;

        await _paymentRepository.UpdateAsync(payment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_Process)]
    public async Task<PaymentDto> CompleteAsync(Guid id)
    {
        await AssertNotFamilyCallerAsync();

        var payment = await _paymentRepository
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == AbpSession.TenantId);

        if (payment == null)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentNotFound,
                "Payment not found.");

        try
        {
            payment.MarkAsCompleted();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(FinancialExceptionCodes.InvalidPaymentStatusTransition, ex.Message);
        }

        await _paymentRepository.UpdateAsync(payment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_Process)]
    public async Task<PaymentDto> FailAsync(Guid id)
    {
        await AssertNotFamilyCallerAsync();

        var payment = await _paymentRepository
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == AbpSession.TenantId);

        if (payment == null)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentNotFound,
                "Payment not found.");

        try
        {
            payment.MarkAsFailed();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(FinancialExceptionCodes.InvalidPaymentStatusTransition, ex.Message);
        }

        await _paymentRepository.UpdateAsync(payment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_Void)]
    public async Task<PaymentDto> RefundAsync(Guid id)
    {
        var payment = await _paymentRepository
            .GetAll()
            .Include(p => p.PaymentAllocations).ThenInclude(pa => pa.StudentFee)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == AbpSession.TenantId);

        if (payment == null)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentNotFound,
                "Payment not found.");

        try
        {
            payment.Refund();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(FinancialExceptionCodes.InvalidPaymentStatusTransition, ex.Message);
        }

        // The family has the money back, so it no longer pays the fees it was
        // put against. Refunding only flipped the payment's status, which left
        // each fee showing as paid. The allocations stay as the record of what
        // the payment had covered.
        foreach (var allocation in payment.PaymentAllocations)
            allocation.StudentFee?.ReversePayment(allocation.Amount);

        await _paymentRepository.UpdateAsync(payment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_Void)]
    public async Task<PaymentDto> CancelAsync(Guid id)
    {
        var payment = await _paymentRepository
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == AbpSession.TenantId);

        if (payment == null)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentNotFound,
                "Payment not found.");

        try
        {
            payment.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(FinancialExceptionCodes.InvalidPaymentStatusTransition, ex.Message);
        }

        await _paymentRepository.UpdateAsync(payment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_Receipts_View)]
    public async Task<PaymentDto> GetByReceiptNumberAsync(string receiptNumber)
    {
        if (string.IsNullOrWhiteSpace(receiptNumber))
            throw new UserFriendlyException(FinancialExceptionCodes.ReceiptNumberRequired,
                "Receipt number is required.");

        var payment = await _paymentRepository
            .GetAll()
            .Include(p => p.Student)
            .Include(p => p.Parent)
            .Include(p => p.PaymentAllocations)
            .FirstOrDefaultAsync(p => p.TenantId == AbpSession.TenantId
                && p.ReceiptNumber == receiptNumber.Trim());

        if (payment == null)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentNotFound,
                "Payment not found for the given receipt number.");

        // Receipt numbers are sequential (REC-000001, …), so without this a
        // parent could read every payment in the school by counting.
        await AssertInFamilyScopeAsync(payment);

        return ObjectMapper.Map<PaymentDto>(payment);
    }

    private async Task<string> GenerateReceiptNumberAsync()
    {
        var receiptNumbers = await _paymentRepository
            .GetAll()
            .Where(p => p.TenantId == AbpSession.TenantId && p.ReceiptNumber != null)
            .Select(p => p.ReceiptNumber)
            .ToListAsync();

        int maxNumber = 0;
        foreach (var rn in receiptNumbers)
        {
            if (rn.StartsWith("REC-") && int.TryParse(rn.Substring(4), out int parsed) && parsed > maxNumber)
            {
                maxNumber = parsed;
            }
        }

        return $"REC-{maxNumber + 1:D6}";
    }
}
