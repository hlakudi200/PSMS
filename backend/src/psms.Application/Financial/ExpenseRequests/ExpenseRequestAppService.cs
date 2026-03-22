using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Authorization.Users;
using psms.Domain.Financial.Entities;
using psms.Domain.Shared.Enums;
using psms.Financial.ExpenseRequests.Dto;
using psms.Financial.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Financial.ExpenseRequests;

/// <summary>
/// Service for managing expense requests.
/// </summary>
[AbpAuthorize(PermissionNames.Financial_Expenses)]
public class ExpenseRequestAppService : ApplicationService, IExpenseRequestAppService
{
    private readonly IRepository<ExpenseRequest, Guid> _expenseRepository;
    private readonly UserManager _userManager;

    public ExpenseRequestAppService(
        IRepository<ExpenseRequest, Guid> expenseRepository,
        UserManager userManager)
    {
        _expenseRepository = expenseRepository;
        _userManager = userManager;
    }

    [AbpAuthorize(PermissionNames.Financial_Expenses_View)]
    public async Task<ExpenseRequestDto> GetAsync(Guid id)
    {
        var expense = await _expenseRepository
            .GetAll()
            .Include(e => e.AcademicYear)
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == AbpSession.TenantId);

        if (expense == null)
            throw new UserFriendlyException(FinancialExceptionCodes.ExpenseNotFound,
                "Expense request not found.");

        return ObjectMapper.Map<ExpenseRequestDto>(expense);
    }

    [AbpAuthorize(PermissionNames.Financial_Expenses_View)]
    public async Task<PagedResultDto<ExpenseRequestListDto>> GetAllAsync(GetExpenseRequestsInput input)
    {
        var query = _expenseRepository
            .GetAll()
            .Where(e => e.TenantId == AbpSession.TenantId)
            .WhereIf(input.AcademicYearId.HasValue, e => e.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.Category.HasValue, e => (int)e.Category == input.Category.Value)
            .WhereIf(input.Status.HasValue, e => (int)e.Status == input.Status.Value)
            .WhereIf(input.Priority.HasValue, e => (int)e.Priority == input.Priority.Value)
            .WhereIf(input.RequestedByUserId.HasValue, e => e.RequestedByUserId == input.RequestedByUserId.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                e => e.RequestNumber.ToLower().Contains(input.Search.Trim().ToLower())
                    || e.Description.ToLower().Contains(input.Search.Trim().ToLower())
                    || e.RequestedByName.ToLower().Contains(input.Search.Trim().ToLower())
                    || (e.Vendor != null && e.Vendor.ToLower().Contains(input.Search.Trim().ToLower())));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<ExpenseRequestListDto>(
            totalCount,
            ObjectMapper.Map<List<ExpenseRequestListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Financial_Expenses_Create)]
    public async Task<ExpenseRequestDto> CreateAsync(CreateExpenseRequestDto input)
    {
        // Get current user name
        var user = await _userManager.GetUserByIdAsync(AbpSession.UserId.Value);
        var requestedByName = user.FullName ?? $"{user.Name} {user.Surname}";

        // Generate request number: EXP-{tenantId:D3}-{yyyy}-{sequential}
        var requestNumber = await GenerateRequestNumberAsync();

        var expense = new ExpenseRequest(
            Guid.NewGuid(),
            AbpSession.TenantId,
            requestNumber,
            AbpSession.UserId.Value,
            requestedByName,
            input.AcademicYearId,
            (ExpenseCategory)input.Category,
            input.Description,
            input.Amount,
            (ExpensePriority)input.Priority)
        {
            Department = input.Department,
            Vendor = input.Vendor,
            QuotationUrl = input.QuotationUrl,
            RequiredByDate = input.RequiredByDate
        };

        await _expenseRepository.InsertAsync(expense);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(expense.Id);
    }

    [AbpAuthorize(PermissionNames.Financial_Expenses_Create)]
    public async Task<ExpenseRequestDto> UpdateAsync(Guid id, UpdateExpenseRequestDto input)
    {
        var expense = await _expenseRepository
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == AbpSession.TenantId);

        if (expense == null)
            throw new UserFriendlyException(FinancialExceptionCodes.ExpenseNotFound,
                "Expense request not found.");

        if (input.Category.HasValue) expense.Category = (ExpenseCategory)input.Category.Value;
        if (input.Description != null) expense.Description = input.Description;
        if (input.Amount.HasValue) expense.Amount = input.Amount.Value;
        if (input.Priority.HasValue) expense.Priority = (ExpensePriority)input.Priority.Value;
        if (input.Department != null) expense.Department = input.Department;
        if (input.Vendor != null) expense.Vendor = input.Vendor;
        if (input.QuotationUrl != null) expense.QuotationUrl = input.QuotationUrl;
        if (input.RequiredByDate.HasValue) expense.RequiredByDate = input.RequiredByDate.Value;

        await _expenseRepository.UpdateAsync(expense);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_Expenses_Create)]
    public async Task DeleteAsync(Guid id)
    {
        var expense = await _expenseRepository
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == AbpSession.TenantId);

        if (expense == null)
            throw new UserFriendlyException(FinancialExceptionCodes.ExpenseNotFound,
                "Expense request not found.");

        await _expenseRepository.DeleteAsync(expense);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Financial_Expenses_Create)]
    public async Task<ExpenseRequestDto> SubmitAsync(Guid id)
    {
        var expense = await _expenseRepository
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == AbpSession.TenantId);

        if (expense == null)
            throw new UserFriendlyException(FinancialExceptionCodes.ExpenseNotFound,
                "Expense request not found.");

        try
        {
            expense.Submit();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(FinancialExceptionCodes.ExpenseInvalidStatus, ex.Message);
        }

        await _expenseRepository.UpdateAsync(expense);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_Expenses_Approve)]
    public async Task<ExpenseRequestDto> ApproveAsync(Guid id, decimal approvedAmount)
    {
        var expense = await _expenseRepository
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == AbpSession.TenantId);

        if (expense == null)
            throw new UserFriendlyException(FinancialExceptionCodes.ExpenseNotFound,
                "Expense request not found.");

        try
        {
            expense.Approve(AbpSession.UserId.Value, approvedAmount);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(FinancialExceptionCodes.ExpenseInvalidStatus, ex.Message);
        }

        await _expenseRepository.UpdateAsync(expense);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_Expenses_Approve)]
    public async Task<ExpenseRequestDto> RejectAsync(Guid id, string reason)
    {
        var expense = await _expenseRepository
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == AbpSession.TenantId);

        if (expense == null)
            throw new UserFriendlyException(FinancialExceptionCodes.ExpenseNotFound,
                "Expense request not found.");

        try
        {
            expense.Reject(AbpSession.UserId.Value, reason);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(FinancialExceptionCodes.ExpenseInvalidStatus, ex.Message);
        }

        await _expenseRepository.UpdateAsync(expense);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_Expenses_Create)]
    public async Task<ExpenseRequestDto> CancelAsync(Guid id)
    {
        var expense = await _expenseRepository
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == AbpSession.TenantId);

        if (expense == null)
            throw new UserFriendlyException(FinancialExceptionCodes.ExpenseNotFound,
                "Expense request not found.");

        try
        {
            expense.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(FinancialExceptionCodes.ExpenseInvalidStatus, ex.Message);
        }

        await _expenseRepository.UpdateAsync(expense);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_Expenses_Approve)]
    public async Task<ExpenseRequestDto> MarkAsPaidAsync(Guid id, string paymentReference)
    {
        var expense = await _expenseRepository
            .FirstOrDefaultAsync(e => e.Id == id && e.TenantId == AbpSession.TenantId);

        if (expense == null)
            throw new UserFriendlyException(FinancialExceptionCodes.ExpenseNotFound,
                "Expense request not found.");

        try
        {
            expense.MarkAsPaid(paymentReference);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(FinancialExceptionCodes.ExpenseInvalidStatus, ex.Message);
        }

        await _expenseRepository.UpdateAsync(expense);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    private async Task<string> GenerateRequestNumberAsync()
    {
        var tenantId = AbpSession.TenantId ?? 0;
        var year = DateTime.UtcNow.Year;
        var prefix = $"EXP-{tenantId:D3}-{year}-";

        var existingNumbers = await _expenseRepository
            .GetAll()
            .Where(e => e.TenantId == AbpSession.TenantId && e.RequestNumber.StartsWith(prefix))
            .Select(e => e.RequestNumber)
            .ToListAsync();

        int maxSequential = 0;
        foreach (var num in existingNumbers)
        {
            var suffix = num.Substring(prefix.Length);
            if (int.TryParse(suffix, out int parsed) && parsed > maxSequential)
            {
                maxSequential = parsed;
            }
        }

        return $"{prefix}{maxSequential + 1:D4}";
    }
}
