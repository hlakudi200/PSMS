using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.MedicalInfos.Dto;
using psms.Academic.Shared;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Academic.MedicalInfos;

[AbpAuthorize(PermissionNames.Academic_Students)]
public class MedicalInfoAppService : ApplicationService, IMedicalInfoAppService
{
    private readonly IRepository<MedicalInfo, Guid> _medicalInfoRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly psms.Academic.Students.ICurrentStudentResolver _currentStudent;
    private readonly psms.Academic.Parents.ICurrentParentResolver _currentParent;

    public MedicalInfoAppService(
        IRepository<MedicalInfo, Guid> medicalInfoRepository,
        IRepository<Student, Guid> studentRepository,
        psms.Academic.Students.ICurrentStudentResolver currentStudent,
        psms.Academic.Parents.ICurrentParentResolver currentParent)
    {
        _medicalInfoRepository = medicalInfoRepository;
        _studentRepository = studentRepository;
        _currentStudent = currentStudent;
        _currentParent = currentParent;
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<MedicalInfoDto> GetByStudentAsync(Guid studentId)
    {
        // LC-08: a student-portal user may only read their own medical info.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != studentId)
            throw new UserFriendlyException(AcademicExceptionCodes.MedicalInfoNotFound, "Medical information not found for this student.");

        // MOB-BE-04: a parent may only read their own children's medical info.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(studentId))
            throw new UserFriendlyException(AcademicExceptionCodes.MedicalInfoNotFound, "Medical information not found for this student.");

        var medicalInfo = await _medicalInfoRepository
            .GetAll()
            .Include(mi => mi.Student)
            .FirstOrDefaultAsync(mi => mi.StudentId == studentId && mi.TenantId == AbpSession.TenantId);

        if (medicalInfo == null)
            throw new UserFriendlyException(AcademicExceptionCodes.MedicalInfoNotFound, "Medical information not found for this student.");

        return ObjectMapper.Map<MedicalInfoDto>(medicalInfo);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_ManageMedicalInfo)]
    public async Task<MedicalInfoDto> CreateOrUpdateAsync(CreateUpdateMedicalInfoDto input)
    {
        // Validate student exists and belongs to tenant
        var student = await _studentRepository
            .FirstOrDefaultAsync(s => s.Id == input.StudentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");

        // Validate LastPhysicalExamDate is not in the future
        if (input.LastPhysicalExamDate.HasValue && input.LastPhysicalExamDate.Value.Date > DateTime.Today)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidPhysicalExamDate,
                "Last physical exam date cannot be in the future.");

        // Check for existing record (upsert pattern)
        var existing = await _medicalInfoRepository
            .FirstOrDefaultAsync(mi => mi.StudentId == input.StudentId && mi.TenantId == AbpSession.TenantId);

        if (existing != null)
        {
            // Update existing
            existing.BloodType = input.BloodType;
            existing.Allergies = input.Allergies;
            existing.ChronicConditions = input.ChronicConditions;
            existing.CurrentMedications = input.CurrentMedications;
            existing.DietaryRequirements = input.DietaryRequirements;
            existing.SpecialNeeds = input.SpecialNeeds;
            existing.DoctorName = input.DoctorName;
            existing.DoctorPhone = input.DoctorPhone;
            existing.MedicalAidProvider = input.MedicalAidProvider;
            existing.MedicalAidNumber = input.MedicalAidNumber;
            existing.MedicalAidPlan = input.MedicalAidPlan;
            existing.MedicalAidMainMember = input.MedicalAidMainMember;
            existing.ImmunizationStatus = input.ImmunizationStatus;
            existing.LastPhysicalExamDate = input.LastPhysicalExamDate;
            existing.CanReceiveOTCMedication = input.CanReceiveOTCMedication;
            existing.AdditionalNotes = input.AdditionalNotes;

            await _medicalInfoRepository.UpdateAsync(existing);
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetByStudentAsync(input.StudentId);
        }

        // Create new
        var medicalInfo = new MedicalInfo(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.StudentId)
        {
            BloodType = input.BloodType,
            Allergies = input.Allergies,
            ChronicConditions = input.ChronicConditions,
            CurrentMedications = input.CurrentMedications,
            DietaryRequirements = input.DietaryRequirements,
            SpecialNeeds = input.SpecialNeeds,
            DoctorName = input.DoctorName,
            DoctorPhone = input.DoctorPhone,
            MedicalAidProvider = input.MedicalAidProvider,
            MedicalAidNumber = input.MedicalAidNumber,
            MedicalAidPlan = input.MedicalAidPlan,
            MedicalAidMainMember = input.MedicalAidMainMember,
            ImmunizationStatus = input.ImmunizationStatus,
            LastPhysicalExamDate = input.LastPhysicalExamDate,
            CanReceiveOTCMedication = input.CanReceiveOTCMedication,
            AdditionalNotes = input.AdditionalNotes
        };

        await _medicalInfoRepository.InsertAsync(medicalInfo);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetByStudentAsync(input.StudentId);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_ManageMedicalInfo)]
    public async Task DeleteAsync(Guid studentId)
    {
        var medicalInfo = await _medicalInfoRepository
            .FirstOrDefaultAsync(mi => mi.StudentId == studentId && mi.TenantId == AbpSession.TenantId);

        if (medicalInfo == null)
            throw new UserFriendlyException(AcademicExceptionCodes.MedicalInfoNotFound, "Medical information not found for this student.");

        await _medicalInfoRepository.DeleteAsync(medicalInfo);
    }
}
