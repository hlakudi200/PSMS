using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.SASpecific.Entities;
using psms.Domain.Shared.Enums;
using psms.SASpecific.Shared;
using psms.SASpecific.StudentTransports.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.SASpecific.StudentTransports;

/// <summary>
/// Service for managing student transport enrollments.
/// </summary>
[AbpAuthorize(PermissionNames.SASpecific_Transport)]
public class StudentTransportAppService : ApplicationService, IStudentTransportAppService
{
    private readonly IRepository<StudentTransport, Guid> _studentTransportRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<SchoolTransport, Guid> _schoolTransportRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;

    public StudentTransportAppService(
        IRepository<StudentTransport, Guid> studentTransportRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<SchoolTransport, Guid> schoolTransportRepository,
        IRepository<AcademicYear, Guid> academicYearRepository)
    {
        _studentTransportRepository = studentTransportRepository;
        _studentRepository = studentRepository;
        _schoolTransportRepository = schoolTransportRepository;
        _academicYearRepository = academicYearRepository;
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_View)]
    public async Task<StudentTransportDto> GetAsync(Guid id)
    {
        var enrollment = await _studentTransportRepository
            .GetAll()
            .Include(st => st.Student)
            .Include(st => st.SchoolTransport)
            .Include(st => st.AcademicYear)
            .FirstOrDefaultAsync(st => st.Id == id && st.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentTransportNotFound,
                "Student transport enrollment not found.");

        return ObjectMapper.Map<StudentTransportDto>(enrollment);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_View)]
    public async Task<PagedResultDto<StudentTransportListDto>> GetAllAsync(GetStudentTransportsInput input)
    {
        var query = _studentTransportRepository
            .GetAll()
            .Include(st => st.Student)
            .Include(st => st.SchoolTransport)
            .Include(st => st.AcademicYear)
            .Where(st => st.TenantId == AbpSession.TenantId)
            .WhereIf(input.StudentId.HasValue, st => st.StudentId == input.StudentId.Value)
            .WhereIf(input.SchoolTransportId.HasValue, st => st.SchoolTransportId == input.SchoolTransportId.Value)
            .WhereIf(input.AcademicYearId.HasValue, st => st.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.Status.HasValue, st => st.Status == input.Status.Value)
            .WhereIf(input.Direction.HasValue, st => st.Direction == input.Direction.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "StartDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<StudentTransportListDto>(
            totalCount,
            ObjectMapper.Map<List<StudentTransportListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_View)]
    public async Task<ListResultDto<StudentTransportListDto>> GetByTransportAsync(Guid transportId)
    {
        var items = await _studentTransportRepository
            .GetAll()
            .Include(st => st.Student)
            .Include(st => st.SchoolTransport)
            .Include(st => st.AcademicYear)
            .Where(st => st.TenantId == AbpSession.TenantId
                && st.SchoolTransportId == transportId)
            .OrderBy(st => st.Student.LastName)
            .ToListAsync();

        return new ListResultDto<StudentTransportListDto>(
            ObjectMapper.Map<List<StudentTransportListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_View)]
    public async Task<ListResultDto<StudentTransportListDto>> GetByStudentAsync(Guid studentId)
    {
        var items = await _studentTransportRepository
            .GetAll()
            .Include(st => st.Student)
            .Include(st => st.SchoolTransport)
            .Include(st => st.AcademicYear)
            .Where(st => st.TenantId == AbpSession.TenantId
                && st.StudentId == studentId)
            .OrderBy(st => st.StartDate)
            .ToListAsync();

        return new ListResultDto<StudentTransportListDto>(
            ObjectMapper.Map<List<StudentTransportListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_Manage)]
    public async Task<StudentTransportDto> CreateAsync(CreateStudentTransportDto input)
    {
        // Validate Student exists
        var student = await _studentRepository
            .FirstOrDefaultAsync(s => s.Id == input.StudentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentNotFound,
                "Student not found.");

        // Validate SchoolTransport exists and is active
        var transport = await _schoolTransportRepository
            .FirstOrDefaultAsync(st => st.Id == input.SchoolTransportId && st.TenantId == AbpSession.TenantId);

        if (transport == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.SchoolTransportNotFound,
                "School transport route not found.");

        if (!transport.IsActive)
            throw new UserFriendlyException(SASpecificExceptionCodes.TransportInactive,
                "Transport route is not active.");

        // Validate AcademicYear exists
        var academicYear = await _academicYearRepository
            .FirstOrDefaultAsync(ay => ay.Id == input.AcademicYearId && ay.TenantId == AbpSession.TenantId);

        if (academicYear == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.AcademicYearNotFound,
                "Academic year not found.");

        // Duplicate check: (StudentId, SchoolTransportId, AcademicYearId)
        var duplicateExists = await _studentTransportRepository
            .GetAll()
            .AnyAsync(st => st.TenantId == AbpSession.TenantId
                && st.StudentId == input.StudentId
                && st.SchoolTransportId == input.SchoolTransportId
                && st.AcademicYearId == input.AcademicYearId);

        if (duplicateExists)
            throw new UserFriendlyException(SASpecificExceptionCodes.DuplicateStudentTransport,
                "This student is already enrolled in this transport route for the selected academic year.");

        // Enroll student (checks capacity)
        try
        {
            transport.EnrollStudent();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(SASpecificExceptionCodes.TransportAtCapacity,
                "Transport route is at full capacity.");
        }

        var enrollment = new StudentTransport(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.StudentId,
            input.SchoolTransportId,
            input.AcademicYearId,
            input.Direction,
            input.StartDate)
        {
            PickupAddress = input.PickupAddress?.Trim(),
            DropoffAddress = input.DropoffAddress?.Trim(),
            PickupTime = input.PickupTime,
            EmergencyContactName = input.EmergencyContactName?.Trim(),
            EmergencyContactPhone = input.EmergencyContactPhone?.Trim(),
            Notes = input.Notes?.Trim()
        };

        await _schoolTransportRepository.UpdateAsync(transport);
        await _studentTransportRepository.InsertAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(enrollment.Id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_Manage)]
    public async Task<StudentTransportDto> UpdateAsync(Guid id, UpdateStudentTransportDto input)
    {
        var enrollment = await _studentTransportRepository
            .FirstOrDefaultAsync(st => st.Id == id && st.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentTransportNotFound,
                "Student transport enrollment not found.");

        if (input.Direction.HasValue) enrollment.Direction = input.Direction.Value;
        if (input.PickupAddress != null) enrollment.PickupAddress = input.PickupAddress.Trim();
        if (input.DropoffAddress != null) enrollment.DropoffAddress = input.DropoffAddress.Trim();
        if (input.PickupTime.HasValue) enrollment.PickupTime = input.PickupTime.Value;
        if (input.EmergencyContactName != null) enrollment.EmergencyContactName = input.EmergencyContactName.Trim();
        if (input.EmergencyContactPhone != null) enrollment.EmergencyContactPhone = input.EmergencyContactPhone.Trim();
        if (input.Notes != null) enrollment.Notes = input.Notes.Trim();

        await _studentTransportRepository.UpdateAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var enrollment = await _studentTransportRepository
            .FirstOrDefaultAsync(st => st.Id == id && st.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentTransportNotFound,
                "Student transport enrollment not found.");

        // Remove student from transport count
        var transport = await _schoolTransportRepository
            .FirstOrDefaultAsync(st => st.Id == enrollment.SchoolTransportId && st.TenantId == AbpSession.TenantId);

        if (transport != null)
        {
            transport.RemoveStudent();
            await _schoolTransportRepository.UpdateAsync(transport);
        }

        await _studentTransportRepository.DeleteAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_Manage)]
    public async Task<StudentTransportDto> SuspendAsync(Guid id)
    {
        var enrollment = await _studentTransportRepository
            .FirstOrDefaultAsync(st => st.Id == id && st.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentTransportNotFound,
                "Student transport enrollment not found.");

        if (enrollment.Status == EnrollmentStatus.Terminated)
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidEnrollmentStatusTransition,
                "Cannot suspend a terminated enrollment.");

        try
        {
            enrollment.Suspend();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidEnrollmentStatusTransition,
                "Invalid enrollment status transition.");
        }

        await _studentTransportRepository.UpdateAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_Manage)]
    public async Task<StudentTransportDto> ReactivateAsync(Guid id)
    {
        var enrollment = await _studentTransportRepository
            .FirstOrDefaultAsync(st => st.Id == id && st.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentTransportNotFound,
                "Student transport enrollment not found.");

        if (enrollment.Status == EnrollmentStatus.Terminated)
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidEnrollmentStatusTransition,
                "Cannot reactivate a terminated enrollment.");

        try
        {
            enrollment.Reactivate();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidEnrollmentStatusTransition,
                "Invalid enrollment status transition.");
        }

        await _studentTransportRepository.UpdateAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Transport_Manage)]
    public async Task<StudentTransportDto> TerminateAsync(Guid id)
    {
        var enrollment = await _studentTransportRepository
            .FirstOrDefaultAsync(st => st.Id == id && st.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentTransportNotFound,
                "Student transport enrollment not found.");

        if (enrollment.Status == EnrollmentStatus.Terminated)
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidEnrollmentStatusTransition,
                "Enrollment is already terminated.");

        try
        {
            enrollment.Terminate();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidEnrollmentStatusTransition,
                "Invalid enrollment status transition.");
        }

        // Remove student from transport count
        var transport = await _schoolTransportRepository
            .FirstOrDefaultAsync(st => st.Id == enrollment.SchoolTransportId && st.TenantId == AbpSession.TenantId);

        if (transport != null)
        {
            transport.RemoveStudent();
            await _schoolTransportRepository.UpdateAsync(transport);
        }

        await _studentTransportRepository.UpdateAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
