using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Financial.Entities;
using psms.Domain.Shared.Enums;
using psms.Financial.PaymentAllocations.Dto;
using psms.Financial.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Financial.PaymentAllocations;

/// <summary>
/// Service for managing payment allocations.
/// </summary>
[AbpAuthorize(PermissionNames.Financial_Payments)]
public class PaymentAllocationAppService : ApplicationService, IPaymentAllocationAppService
{
    private readonly IRepository<PaymentAllocation, Guid> _paymentAllocationRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<StudentFee, Guid> _studentFeeRepository;

    public PaymentAllocationAppService(
        IRepository<PaymentAllocation, Guid> paymentAllocationRepository,
        IRepository<Payment, Guid> paymentRepository,
        IRepository<StudentFee, Guid> studentFeeRepository)
    {
        _paymentAllocationRepository = paymentAllocationRepository;
        _paymentRepository = paymentRepository;
        _studentFeeRepository = studentFeeRepository;
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_View)]
    public async Task<PaymentAllocationDto> GetAsync(Guid id)
    {
        var allocation = await _paymentAllocationRepository
            .GetAll()
            .Include(pa => pa.Payment)
            .Include(pa => pa.StudentFee).ThenInclude(sf => sf.FeeStructure)
            .Include(pa => pa.StudentFee).ThenInclude(sf => sf.Student)
            .Where(pa => pa.Payment.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(pa => pa.Id == id);

        if (allocation == null)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentAllocationNotFound,
                "Payment allocation not found.");

        return ObjectMapper.Map<PaymentAllocationDto>(allocation);
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_View)]
    public async Task<ListResultDto<PaymentAllocationListDto>> GetByPaymentAsync(Guid paymentId)
    {
        var items = await _paymentAllocationRepository
            .GetAll()
            .Include(pa => pa.Payment)
            .Include(pa => pa.StudentFee).ThenInclude(sf => sf.FeeStructure)
            .Include(pa => pa.StudentFee).ThenInclude(sf => sf.Student)
            .Where(pa => pa.Payment.TenantId == AbpSession.TenantId && pa.PaymentId == paymentId)
            .OrderBy(pa => pa.AllocatedDate)
            .ToListAsync();

        return new ListResultDto<PaymentAllocationListDto>(
            ObjectMapper.Map<List<PaymentAllocationListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_View)]
    public async Task<ListResultDto<PaymentAllocationListDto>> GetByStudentFeeAsync(Guid studentFeeId)
    {
        var items = await _paymentAllocationRepository
            .GetAll()
            .Include(pa => pa.Payment)
            .Include(pa => pa.StudentFee).ThenInclude(sf => sf.FeeStructure)
            .Include(pa => pa.StudentFee).ThenInclude(sf => sf.Student)
            .Where(pa => pa.Payment.TenantId == AbpSession.TenantId && pa.StudentFeeId == studentFeeId)
            .OrderBy(pa => pa.AllocatedDate)
            .ToListAsync();

        return new ListResultDto<PaymentAllocationListDto>(
            ObjectMapper.Map<List<PaymentAllocationListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_Reconcile)]
    public async Task<PaymentAllocationDto> CreateAsync(CreatePaymentAllocationDto input)
    {
        // Load payment with allocations for unallocated amount calculation
        var payment = await _paymentRepository
            .GetAll()
            .Include(p => p.PaymentAllocations)
            .FirstOrDefaultAsync(p => p.Id == input.PaymentId && p.TenantId == AbpSession.TenantId);

        if (payment == null)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentNotFound,
                "Payment not found.");

        if (payment.Status != PaymentStatus.Completed)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentNotCompleted,
                "Payment must be completed before allocating.");

        // Load student fee
        var studentFee = await _studentFeeRepository
            .FirstOrDefaultAsync(sf => sf.Id == input.StudentFeeId && sf.TenantId == AbpSession.TenantId);

        if (studentFee == null)
            throw new UserFriendlyException(FinancialExceptionCodes.StudentFeeNotFound,
                "Student fee not found.");

        // Block allocation to settled fees
        if (studentFee.Status == FeeStatus.Waived || studentFee.Status == FeeStatus.Cancelled)
            throw new UserFriendlyException(FinancialExceptionCodes.CannotAllocateToSettledFee,
                "Cannot allocate to a waived or cancelled fee.");

        // Duplicate check: (PaymentId, StudentFeeId)
        var duplicateExists = await _paymentAllocationRepository
            .GetAll()
            .Where(pa => pa.Payment.TenantId == AbpSession.TenantId)
            .AnyAsync(pa => pa.PaymentId == input.PaymentId && pa.StudentFeeId == input.StudentFeeId);

        if (duplicateExists)
            throw new UserFriendlyException(FinancialExceptionCodes.DuplicateAllocation,
                "An allocation already exists for this payment and student fee.");

        // Validate amount doesn't exceed unallocated
        var unallocated = payment.GetUnallocatedAmount();
        if (input.Amount > unallocated)
            throw new UserFriendlyException(FinancialExceptionCodes.AllocationExceedsUnallocated,
                $"Allocation amount ({input.Amount:N2}) exceeds unallocated amount ({unallocated:N2}).");

        // Validate amount doesn't exceed outstanding balance
        var outstanding = studentFee.GetOutstandingBalance();
        if (input.Amount > outstanding)
            throw new UserFriendlyException(FinancialExceptionCodes.AllocationExceedsOutstanding,
                $"Allocation amount ({input.Amount:N2}) exceeds outstanding balance ({outstanding:N2}).");

        var allocation = new PaymentAllocation(
            Guid.NewGuid(),
            input.PaymentId,
            input.StudentFeeId,
            input.Amount);

        // Update student fee payment tracking
        studentFee.RecordPayment(input.Amount);

        await _paymentAllocationRepository.InsertAsync(allocation);
        await _studentFeeRepository.UpdateAsync(studentFee);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(allocation.Id);
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_Reconcile)]
    public async Task<ListResultDto<PaymentAllocationDto>> BulkAllocateAsync(BulkAllocatePaymentDto input)
    {
        // Load payment with allocations
        var payment = await _paymentRepository
            .GetAll()
            .Include(p => p.PaymentAllocations)
            .FirstOrDefaultAsync(p => p.Id == input.PaymentId && p.TenantId == AbpSession.TenantId);

        if (payment == null)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentNotFound,
                "Payment not found.");

        if (payment.Status != PaymentStatus.Completed)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentNotCompleted,
                "Payment must be completed before allocating.");

        // Validate total doesn't exceed unallocated
        var totalAllocationAmount = input.Allocations.Sum(a => a.Amount);
        var unallocated = payment.GetUnallocatedAmount();
        if (totalAllocationAmount > unallocated)
            throw new UserFriendlyException(FinancialExceptionCodes.AllocationExceedsUnallocated,
                $"Total allocation amount ({totalAllocationAmount:N2}) exceeds unallocated amount ({unallocated:N2}).");

        // Check for duplicate StudentFeeIds within the batch
        var studentFeeIds = input.Allocations.Select(a => a.StudentFeeId).ToList();
        if (studentFeeIds.Count != studentFeeIds.Distinct().Count())
            throw new UserFriendlyException(FinancialExceptionCodes.DuplicateStudentFeeInBatch,
                "Duplicate student fee entries in bulk allocation.");

        // Load and validate all student fees
        var studentFees = await _studentFeeRepository
            .GetAll()
            .Where(sf => sf.TenantId == AbpSession.TenantId && studentFeeIds.Contains(sf.Id))
            .ToListAsync();

        if (studentFees.Count != studentFeeIds.Distinct().Count())
            throw new UserFriendlyException(FinancialExceptionCodes.StudentFeeNotFound,
                "One or more student fees not found.");

        // Check for existing allocations
        var existingAllocations = await _paymentAllocationRepository
            .GetAll()
            .Where(pa => pa.Payment.TenantId == AbpSession.TenantId
                && pa.PaymentId == input.PaymentId
                && studentFeeIds.Contains(pa.StudentFeeId))
            .Select(pa => pa.StudentFeeId)
            .ToListAsync();

        if (existingAllocations.Count > 0)
            throw new UserFriendlyException(FinancialExceptionCodes.DuplicateAllocation,
                "One or more allocations already exist for this payment.");

        // Validate individual amounts don't exceed outstanding balances and fee status
        foreach (var entry in input.Allocations)
        {
            var studentFee = studentFees.First(sf => sf.Id == entry.StudentFeeId);

            if (studentFee.Status == FeeStatus.Waived || studentFee.Status == FeeStatus.Cancelled)
                throw new UserFriendlyException(FinancialExceptionCodes.CannotAllocateToSettledFee,
                    "Cannot allocate to a waived or cancelled fee.");

            var outstanding = studentFee.GetOutstandingBalance();
            if (entry.Amount > outstanding)
                throw new UserFriendlyException(FinancialExceptionCodes.AllocationExceedsOutstanding,
                    $"Allocation amount ({entry.Amount:N2}) exceeds outstanding balance ({outstanding:N2}) for student fee.");
        }

        // Create all allocations
        var createdIds = new List<Guid>();
        foreach (var entry in input.Allocations)
        {
            var allocation = new PaymentAllocation(
                Guid.NewGuid(),
                input.PaymentId,
                entry.StudentFeeId,
                entry.Amount);

            var studentFee = studentFees.First(sf => sf.Id == entry.StudentFeeId);
            studentFee.RecordPayment(entry.Amount);

            await _paymentAllocationRepository.InsertAsync(allocation);
            await _studentFeeRepository.UpdateAsync(studentFee);
            createdIds.Add(allocation.Id);
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        var results = new List<PaymentAllocationDto>();
        foreach (var id in createdIds)
        {
            results.Add(await GetAsync(id));
        }

        return new ListResultDto<PaymentAllocationDto>(results);
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_Reconcile)]
    public async Task<PaymentAllocationDto> UpdateAsync(Guid id, UpdatePaymentAllocationDto input)
    {
        var allocation = await _paymentAllocationRepository
            .GetAll()
            .Include(pa => pa.Payment).ThenInclude(p => p.PaymentAllocations)
            .Include(pa => pa.StudentFee)
            .Where(pa => pa.Payment.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(pa => pa.Id == id);

        if (allocation == null)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentAllocationNotFound,
                "Payment allocation not found.");

        // Only allow updates on completed payments
        if (allocation.Payment.Status != PaymentStatus.Completed)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentNotCompleted,
                "Cannot modify allocation for a payment that is not completed.");

        var oldAmount = allocation.Amount;
        var newAmount = input.Amount;

        // Reverse old amount from student fee
        allocation.StudentFee.AmountPaid -= oldAmount;
        if (allocation.StudentFee.AmountPaid < 0)
            allocation.StudentFee.AmountPaid = 0;

        // Validate new amount against payment unallocated (excluding this allocation)
        var unallocated = allocation.Payment.GetUnallocatedAmount() + oldAmount;
        if (newAmount > unallocated)
            throw new UserFriendlyException(FinancialExceptionCodes.AllocationExceedsUnallocated,
                $"New allocation amount ({newAmount:N2}) exceeds available unallocated amount ({unallocated:N2}).");

        // Validate new amount against student fee outstanding (after reversing old amount)
        var outstanding = allocation.StudentFee.GetOutstandingBalance();
        if (newAmount > outstanding)
            throw new UserFriendlyException(FinancialExceptionCodes.AllocationExceedsOutstanding,
                $"New allocation amount ({newAmount:N2}) exceeds outstanding balance ({outstanding:N2}).");

        // Apply new amount
        allocation.UpdateAmount(newAmount);
        allocation.StudentFee.RecordPayment(newAmount);

        await _paymentAllocationRepository.UpdateAsync(allocation);
        await _studentFeeRepository.UpdateAsync(allocation.StudentFee);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_Payments_Reconcile)]
    public async Task DeleteAsync(Guid id)
    {
        var allocation = await _paymentAllocationRepository
            .GetAll()
            .Include(pa => pa.Payment)
            .Include(pa => pa.StudentFee)
            .Where(pa => pa.Payment.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(pa => pa.Id == id);

        if (allocation == null)
            throw new UserFriendlyException(FinancialExceptionCodes.PaymentAllocationNotFound,
                "Payment allocation not found.");

        // Reverse allocation from student fee
        var originalStatus = allocation.StudentFee.Status;
        allocation.StudentFee.AmountPaid -= allocation.Amount;
        if (allocation.StudentFee.AmountPaid < 0)
            allocation.StudentFee.AmountPaid = 0;

        // Preserve Waived/Cancelled status — don't recompute settled fees
        if (originalStatus != FeeStatus.Waived && originalStatus != FeeStatus.Cancelled)
        {
            var outstanding = allocation.StudentFee.GetOutstandingBalance();
            if (outstanding <= 0)
                allocation.StudentFee.Status = FeeStatus.Paid;
            else if (allocation.StudentFee.AmountPaid > 0)
                allocation.StudentFee.Status = FeeStatus.PartiallyPaid;
            else if (DateTime.UtcNow > allocation.StudentFee.DueDate)
                allocation.StudentFee.Status = FeeStatus.Overdue;
            else
                allocation.StudentFee.Status = FeeStatus.Pending;
        }

        await _studentFeeRepository.UpdateAsync(allocation.StudentFee);
        await _paymentAllocationRepository.DeleteAsync(allocation);
        await CurrentUnitOfWork.SaveChangesAsync();
    }
}
