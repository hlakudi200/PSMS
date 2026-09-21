using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using psms.Academic.POPIAConsents.Dto;
using psms.Academic.Shared;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Academic.POPIAConsents;

[AbpAuthorize(PermissionNames.Academic_Students)]
public class POPIAConsentAppService : ApplicationService, IPOPIAConsentAppService
{
    private readonly IRepository<POPIAConsent, Guid> _popiaConsentRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly psms.Academic.Students.ICurrentStudentResolver _currentStudent;
    private readonly psms.Academic.Parents.ICurrentParentResolver _currentParent;

    public POPIAConsentAppService(
        IRepository<POPIAConsent, Guid> popiaConsentRepository,
        IRepository<Student, Guid> studentRepository,
        IHttpContextAccessor httpContextAccessor,
        psms.Academic.Students.ICurrentStudentResolver currentStudent,
        psms.Academic.Parents.ICurrentParentResolver currentParent)
    {
        _popiaConsentRepository = popiaConsentRepository;
        _studentRepository = studentRepository;
        _httpContextAccessor = httpContextAccessor;
        _currentStudent = currentStudent;
        _currentParent = currentParent;
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<POPIAConsentDto> GetByStudentAsync(Guid studentId)
    {
        // LC-08: a student may only read their own POPIA consent.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != studentId)
            throw new UserFriendlyException(AcademicExceptionCodes.POPIAConsentNotFound, "POPIA consent not found for this student.");

        // MOB-BE-04: a parent may only read their own children's POPIA consent.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(studentId))
            throw new UserFriendlyException(AcademicExceptionCodes.POPIAConsentNotFound, "POPIA consent not found for this student.");

        var consent = await _popiaConsentRepository
            .GetAll()
            .Include(pc => pc.Student)
            .FirstOrDefaultAsync(pc => pc.StudentId == studentId && pc.TenantId == AbpSession.TenantId);

        if (consent == null)
            throw new UserFriendlyException(AcademicExceptionCodes.POPIAConsentNotFound, "POPIA consent not found for this student.");

        return ObjectMapper.Map<POPIAConsentDto>(consent);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<PagedResultDto<POPIAConsentDto>> GetAllAsync(GetPOPIAConsentsInput input)
    {
        // LC-08: a student-portal user only ever sees their own consent.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        // MOB-BE-04: a parent only ever sees their own children's consent.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();

        var query = _popiaConsentRepository
            .GetAll()
            .Include(pc => pc.Student)
            .Where(pc => pc.TenantId == AbpSession.TenantId)
            .WhereIf(selfId.HasValue, pc => pc.StudentId == selfId.Value)
            .WhereIf(childIds != null, pc => childIds.Contains(pc.StudentId))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                pc => pc.Student.FirstName.ToLower().Contains(input.Keyword.ToLower())
                  || pc.Student.LastName.ToLower().Contains(input.Keyword.ToLower()))
            .WhereIf(input.StudentId.HasValue, pc => pc.StudentId == input.StudentId.Value)
            .WhereIf(input.AllowPhotography.HasValue, pc => pc.AllowPhotography == input.AllowPhotography.Value)
            .WhereIf(input.AllowDataSharing.HasValue, pc => pc.AllowDataSharing == input.AllowDataSharing.Value)
            .WhereIf(input.AllowNameInPublications.HasValue, pc => pc.AllowNameInPublications == input.AllowNameInPublications.Value)
            .WhereIf(input.AllowMarketingUse.HasValue, pc => pc.AllowMarketingUse == input.AllowMarketingUse.Value);

        var totalCount = await query.CountAsync();

        var consents = await query
            .OrderBy(input.Sorting ?? "ConsentDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<POPIAConsentDto>(
            totalCount,
            ObjectMapper.Map<List<POPIAConsentDto>>(consents));
    }

    [AbpAuthorize(PermissionNames.Academic_Students_ManagePOPIAConsent)]
    public async Task<POPIAConsentDto> CreateAsync(CreatePOPIAConsentDto input)
    {
        // Validate student exists and belongs to tenant
        var student = await _studentRepository
            .FirstOrDefaultAsync(s => s.Id == input.StudentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");

        // Validate consent date is not in the future
        if (input.ConsentDate.Date > DateTime.Today)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidConsentDate, "Consent date cannot be in the future.");

        // Check for existing active consent (1:1 active consent per student)
        var existingConsent = await _popiaConsentRepository
            .FirstOrDefaultAsync(pc => pc.StudentId == input.StudentId && pc.TenantId == AbpSession.TenantId);

        if (existingConsent != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicatePOPIAConsent,
                "An active POPIA consent already exists for this student. Revoke it before creating a new one.");

        var consent = new POPIAConsent(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.StudentId,
            input.ConsentDate,
            input.ParentSignatureUserId)
        {
            AllowPhotography = input.AllowPhotography,
            AllowDataSharing = input.AllowDataSharing,
            AllowNameInPublications = input.AllowNameInPublications,
            AllowMarketingUse = input.AllowMarketingUse,
            IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
        };

        await _popiaConsentRepository.InsertAsync(consent);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetByStudentAsync(input.StudentId);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_ManagePOPIAConsent)]
    public async Task<POPIAConsentDto> UpdateAsync(Guid id, UpdatePOPIAConsentDto input)
    {
        var consent = await _popiaConsentRepository
            .FirstOrDefaultAsync(pc => pc.Id == id && pc.TenantId == AbpSession.TenantId);

        if (consent == null)
            throw new UserFriendlyException(AcademicExceptionCodes.POPIAConsentNotFound, "POPIA consent not found.");

        if (input.AllowPhotography.HasValue) consent.AllowPhotography = input.AllowPhotography.Value;
        if (input.AllowDataSharing.HasValue) consent.AllowDataSharing = input.AllowDataSharing.Value;
        if (input.AllowNameInPublications.HasValue) consent.AllowNameInPublications = input.AllowNameInPublications.Value;
        if (input.AllowMarketingUse.HasValue) consent.AllowMarketingUse = input.AllowMarketingUse.Value;

        await _popiaConsentRepository.UpdateAsync(consent);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetByStudentAsync(consent.StudentId);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_ManagePOPIAConsent)]
    public async Task RevokeAsync(Guid id)
    {
        var consent = await _popiaConsentRepository
            .FirstOrDefaultAsync(pc => pc.Id == id && pc.TenantId == AbpSession.TenantId);

        if (consent == null)
            throw new UserFriendlyException(AcademicExceptionCodes.POPIAConsentNotFound, "POPIA consent not found.");

        await _popiaConsentRepository.DeleteAsync(consent);
    }
}
