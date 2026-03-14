using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.Shared.Enums;
using psms.Financial.Shared;
using psms.Financial.StudentFees.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Financial.StudentFees;

/// <summary>
/// Service for managing student fees.
/// </summary>
[AbpAuthorize(PermissionNames.Financial_FeeStructures)]
public class StudentFeeAppService : ApplicationService, IStudentFeeAppService
{
    private readonly IRepository<StudentFee, Guid> _studentFeeRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<FeeStructure, Guid> _feeStructureRepository;

    public StudentFeeAppService(
        IRepository<StudentFee, Guid> studentFeeRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<FeeStructure, Guid> feeStructureRepository)
    {
        _studentFeeRepository = studentFeeRepository;
        _studentRepository = studentRepository;
        _feeStructureRepository = feeStructureRepository;
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_View)]
    public async Task<StudentFeeDto> GetAsync(Guid id)
    {
        var studentFee = await _studentFeeRepository
            .GetAll()
            .Include(sf => sf.Student)
            .Include(sf => sf.FeeStructure)
            .Include(sf => sf.PaymentAllocations)
            .FirstOrDefaultAsync(sf => sf.Id == id && sf.TenantId == AbpSession.TenantId);

        if (studentFee == null)
            throw new UserFriendlyException(FinancialExceptionCodes.StudentFeeNotFound,
                "Student fee not found.");

        return ObjectMapper.Map<StudentFeeDto>(studentFee);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_View)]
    public async Task<PagedResultDto<StudentFeeListDto>> GetAllAsync(GetStudentFeesInput input)
    {
        var query = _studentFeeRepository
            .GetAll()
            .Include(sf => sf.Student)
            .Include(sf => sf.FeeStructure)
            .Where(sf => sf.TenantId == AbpSession.TenantId)
            .WhereIf(input.StudentId.HasValue, sf => sf.StudentId == input.StudentId.Value)
            .WhereIf(input.FeeStructureId.HasValue, sf => sf.FeeStructureId == input.FeeStructureId.Value)
            .WhereIf(input.GradeId.HasValue, sf => sf.FeeStructure.GradeId == input.GradeId.Value)
            .WhereIf(input.AcademicYearId.HasValue, sf => sf.FeeStructure.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.Status.HasValue, sf => sf.Status == input.Status.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.StudentName),
                sf => (sf.Student.FirstName + " " + sf.Student.LastName).ToLower()
                    .Contains(input.StudentName.Trim().ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                sf => sf.Student.FirstName.ToLower().Contains(input.Keyword.Trim().ToLower())
                    || sf.Student.LastName.ToLower().Contains(input.Keyword.Trim().ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "DueDate ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<StudentFeeListDto>(
            totalCount,
            ObjectMapper.Map<List<StudentFeeListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_View)]
    public async Task<ListResultDto<StudentFeeListDto>> GetByStudentAsync(Guid studentId)
    {
        var items = await _studentFeeRepository
            .GetAll()
            .Include(sf => sf.Student)
            .Include(sf => sf.FeeStructure)
            .Where(sf => sf.TenantId == AbpSession.TenantId && sf.StudentId == studentId)
            .OrderBy(sf => sf.DueDate)
            .ToListAsync();

        return new ListResultDto<StudentFeeListDto>(
            ObjectMapper.Map<List<StudentFeeListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_Create)]
    public async Task<StudentFeeDto> CreateAsync(CreateStudentFeeDto input)
    {
        // Validate Student exists
        var student = await _studentRepository
            .FirstOrDefaultAsync(s => s.Id == input.StudentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(FinancialExceptionCodes.StudentNotFound,
                "Student not found.");

        // Validate FeeStructure exists and is active
        var feeStructure = await _feeStructureRepository
            .FirstOrDefaultAsync(fs => fs.Id == input.FeeStructureId && fs.TenantId == AbpSession.TenantId);

        if (feeStructure == null)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeStructureNotFound,
                "Fee structure not found.");

        if (!feeStructure.IsActive)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeStructureInactive,
                "Fee structure is not active.");

        // Duplicate check: (StudentId, FeeStructureId)
        var duplicateExists = await _studentFeeRepository
            .GetAll()
            .AnyAsync(sf => sf.TenantId == AbpSession.TenantId
                && sf.StudentId == input.StudentId
                && sf.FeeStructureId == input.FeeStructureId);

        if (duplicateExists)
            throw new UserFriendlyException(FinancialExceptionCodes.DuplicateStudentFee,
                "This student already has this fee assigned.");

        var studentFee = new StudentFee(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.StudentId,
            input.FeeStructureId,
            input.AmountDue,
            input.DueDate)
        {
            Notes = input.Notes
        };

        await _studentFeeRepository.InsertAsync(studentFee);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(studentFee.Id);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_Create)]
    public async Task<ListResultDto<StudentFeeDto>> BulkCreateAsync(BulkCreateStudentFeesDto input)
    {
        // Validate FeeStructure exists and is active
        var feeStructure = await _feeStructureRepository
            .FirstOrDefaultAsync(fs => fs.Id == input.FeeStructureId && fs.TenantId == AbpSession.TenantId);

        if (feeStructure == null)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeStructureNotFound,
                "Fee structure not found.");

        if (!feeStructure.IsActive)
            throw new UserFriendlyException(FinancialExceptionCodes.FeeStructureInactive,
                "Fee structure is not active.");

        // Check for duplicates within the batch
        var distinctIds = input.StudentIds.Distinct().ToList();
        if (distinctIds.Count != input.StudentIds.Count)
            throw new UserFriendlyException(FinancialExceptionCodes.DuplicateStudentInBatch,
                "Duplicate student IDs found in the batch.");

        // Validate ALL students exist
        var existingStudentIds = await _studentRepository
            .GetAll()
            .Where(s => s.TenantId == AbpSession.TenantId && distinctIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        if (existingStudentIds.Count != distinctIds.Count)
            throw new UserFriendlyException(FinancialExceptionCodes.StudentNotFound,
                "One or more students not found.");

        // Check for existing assignments
        var existingAssignments = await _studentFeeRepository
            .GetAll()
            .Where(sf => sf.TenantId == AbpSession.TenantId
                && sf.FeeStructureId == input.FeeStructureId
                && distinctIds.Contains(sf.StudentId))
            .Select(sf => sf.StudentId)
            .ToListAsync();

        if (existingAssignments.Count > 0)
            throw new UserFriendlyException(FinancialExceptionCodes.DuplicateStudentFee,
                "One or more students already have this fee assigned.");

        // Determine due date
        var dueDate = input.DueDateOverride
            ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month,
                Math.Min(feeStructure.DueDay, DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month)));

        var createdIds = new List<Guid>();

        foreach (var studentId in distinctIds)
        {
            var studentFee = new StudentFee(
                Guid.NewGuid(),
                AbpSession.TenantId,
                studentId,
                input.FeeStructureId,
                feeStructure.Amount,
                dueDate);

            await _studentFeeRepository.InsertAsync(studentFee);
            createdIds.Add(studentFee.Id);
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        var results = new List<StudentFeeDto>();
        foreach (var id in createdIds)
        {
            results.Add(await GetAsync(id));
        }

        return new ListResultDto<StudentFeeDto>(results);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_Edit)]
    public async Task<StudentFeeDto> UpdateAsync(Guid id, UpdateStudentFeeDto input)
    {
        var studentFee = await _studentFeeRepository
            .FirstOrDefaultAsync(sf => sf.Id == id && sf.TenantId == AbpSession.TenantId);

        if (studentFee == null)
            throw new UserFriendlyException(FinancialExceptionCodes.StudentFeeNotFound,
                "Student fee not found.");

        // Block updates on settled fees
        if (input.AmountDue.HasValue || input.DueDate.HasValue)
        {
            if (studentFee.Status == FeeStatus.Paid || studentFee.Status == FeeStatus.Waived
                || studentFee.Status == FeeStatus.Cancelled)
                throw new UserFriendlyException(FinancialExceptionCodes.CannotModifySettledFee,
                    "Cannot modify amount or due date on a paid, waived, or cancelled fee.");
        }

        if (input.AmountDue.HasValue)
        {
            if (input.AmountDue.Value < studentFee.AmountPaid)
                throw new UserFriendlyException(FinancialExceptionCodes.AmountDueBelowAmountPaid,
                    "Amount due cannot be less than the amount already paid.");
            studentFee.AmountDue = input.AmountDue.Value;
        }

        if (input.DueDate.HasValue) studentFee.DueDate = input.DueDate.Value;
        if (input.Notes != null) studentFee.Notes = input.Notes;

        await _studentFeeRepository.UpdateAsync(studentFee);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var studentFee = await _studentFeeRepository
            .GetAll()
            .Include(sf => sf.PaymentAllocations)
            .FirstOrDefaultAsync(sf => sf.Id == id && sf.TenantId == AbpSession.TenantId);

        if (studentFee == null)
            throw new UserFriendlyException(FinancialExceptionCodes.StudentFeeNotFound,
                "Student fee not found.");

        if (studentFee.PaymentAllocations != null && studentFee.PaymentAllocations.Count > 0)
            throw new UserFriendlyException(FinancialExceptionCodes.CannotDeleteStudentFeeWithPayments,
                "Cannot delete a student fee that has payment allocations.");

        await _studentFeeRepository.DeleteAsync(studentFee);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_Approve)]
    public async Task<StudentFeeDto> ApplyDiscountAsync(Guid id, decimal discountAmount)
    {
        var studentFee = await _studentFeeRepository
            .FirstOrDefaultAsync(sf => sf.Id == id && sf.TenantId == AbpSession.TenantId);

        if (studentFee == null)
            throw new UserFriendlyException(FinancialExceptionCodes.StudentFeeNotFound,
                "Student fee not found.");

        if (studentFee.Status == FeeStatus.Paid || studentFee.Status == FeeStatus.Waived
            || studentFee.Status == FeeStatus.Cancelled)
            throw new UserFriendlyException(FinancialExceptionCodes.CannotModifySettledFee,
                "Cannot apply discount to a paid, waived, or cancelled fee.");

        try
        {
            studentFee.ApplyDiscount(discountAmount);
        }
        catch (ArgumentException ex)
        {
            throw new UserFriendlyException(FinancialExceptionCodes.InvalidDiscountAmount, ex.Message);
        }

        await _studentFeeRepository.UpdateAsync(studentFee);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_Approve)]
    public async Task<StudentFeeDto> WaiveAsync(Guid id)
    {
        var studentFee = await _studentFeeRepository
            .FirstOrDefaultAsync(sf => sf.Id == id && sf.TenantId == AbpSession.TenantId);

        if (studentFee == null)
            throw new UserFriendlyException(FinancialExceptionCodes.StudentFeeNotFound,
                "Student fee not found.");

        if (studentFee.Status == FeeStatus.Paid)
            throw new UserFriendlyException(FinancialExceptionCodes.StudentFeeAlreadyPaid,
                "Cannot waive a fee that is already paid.");

        if (studentFee.Status == FeeStatus.Cancelled)
            throw new UserFriendlyException(FinancialExceptionCodes.InvalidWaiveStatus,
                "Cannot waive a cancelled fee.");

        if (studentFee.Status == FeeStatus.Waived)
            throw new UserFriendlyException(FinancialExceptionCodes.StudentFeeAlreadyWaived,
                "Fee is already waived.");

        studentFee.Waive();
        await _studentFeeRepository.UpdateAsync(studentFee);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_Approve)]
    public async Task<StudentFeeDto> CancelAsync(Guid id)
    {
        var studentFee = await _studentFeeRepository
            .FirstOrDefaultAsync(sf => sf.Id == id && sf.TenantId == AbpSession.TenantId);

        if (studentFee == null)
            throw new UserFriendlyException(FinancialExceptionCodes.StudentFeeNotFound,
                "Student fee not found.");

        if (studentFee.Status != FeeStatus.Pending && studentFee.Status != FeeStatus.Overdue)
            throw new UserFriendlyException(FinancialExceptionCodes.InvalidCancelStatus,
                "Only pending or overdue fees can be cancelled.");

        studentFee.Status = FeeStatus.Cancelled;
        await _studentFeeRepository.UpdateAsync(studentFee);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Financial_FeeStructures_Approve)]
    public async Task CheckOverdueFeesAsync()
    {
        var pendingFees = await _studentFeeRepository
            .GetAll()
            .Where(sf => sf.TenantId == AbpSession.TenantId
                && sf.Status == FeeStatus.Pending
                && sf.DueDate < DateTime.UtcNow)
            .ToListAsync();

        foreach (var fee in pendingFees)
        {
            fee.CheckOverdue();
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }
}
