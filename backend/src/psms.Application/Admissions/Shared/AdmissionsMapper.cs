using System;
using AutoMapper;
using psms.Admissions.AdmissionAssessments.Dto;
using psms.Admissions.AdmissionInterviews.Dto;
using psms.Admissions.AdmissionSettings.Dto;
using psms.Admissions.ApplicantParents.Dto;
using psms.Admissions.ApplicationDocuments.Dto;
using psms.Admissions.ApplicationFees.Dto;
using psms.Admissions.Applications.Dto;
using psms.Admissions.Waitlists.Dto;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using psms.Shared;

namespace psms.Admissions.Shared;

/// <summary>
/// AutoMapper profile for the Admissions module.
/// Maps between domain entities and DTOs.
/// </summary>
public class AdmissionsMapper : Profile
{
    public AdmissionsMapper()
    {
        // Application mappings
        CreateApplicationMappings();

        // ApplicantParent mappings
        CreateApplicantParentMappings();

        // ApplicationDocument mappings
        CreateApplicationDocumentMappings();

        // ApplicationFee mappings
        CreateApplicationFeeMappings();

        // AdmissionInterview mappings
        CreateAdmissionInterviewMappings();

        // AdmissionAssessment mappings
        CreateAdmissionAssessmentMappings();

        // Waitlist mappings
        CreateWaitlistMappings();

        // AdmissionSettings mappings
        CreateAdmissionSettingsMappings();
    }

    private void CreateApplicationMappings()
    {
        // Entity to DTO
        CreateMap<Application, ApplicationDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.ProspectiveStudentFirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.ProspectiveStudentLastName))
            .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.ProspectiveStudentMiddleName))
            .ForMember(dest => dest.AcademicYearName, opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null))
            .ForMember(dest => dest.ApplyingForGradeName, opt => opt.MapFrom(src => src.AppliedGrade != null ? src.AppliedGrade.GradeName : null))
            .ForMember(dest => dest.ApplyingForGradeId, opt => opt.MapFrom(src => src.AppliedGradeId))
            .ForMember(dest => dest.ParentCount, opt => opt.MapFrom(src => src.ApplicantParents != null ? src.ApplicantParents.Count : 0))
            .ForMember(dest => dest.DocumentCount, opt => opt.MapFrom(src => src.ApplicationDocuments != null ? src.ApplicationDocuments.Count : 0))
            .ForMember(dest => dest.SubmittedDate, opt => opt.MapFrom(src => src.SubmissionDate))
            .ForMember(dest => dest.OfferExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
            .ForMember(dest => dest.ReviewedByUserName, opt => opt.Ignore()) // Set in service
            .ForMember(dest => dest.HasInterview, opt => opt.MapFrom(src => src.AdmissionInterview != null))
            .ForMember(dest => dest.HasAssessment, opt => opt.MapFrom(src => src.AdmissionAssessment != null))
            .ForMember(dest => dest.WaitlistPosition, opt => opt.MapFrom(src => src.Waitlist != null ? src.Waitlist.Position : (int?)null))
            .ForMember(dest => dest.IsFeePaid, opt => opt.MapFrom(src => src.ApplicationFee != null && src.ApplicationFee.Status == PaymentStatus.Completed))
            .ForMember(dest => dest.IdNumber, opt => opt.MapFrom(src => PiiMasking.Mask(src.IdNumber)))
            .ForMember(dest => dest.PassportNumber, opt => opt.MapFrom(src => PiiMasking.Mask(src.PassportNumber)));

        CreateMap<Application, ApplicationListDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.ProspectiveStudentFirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.ProspectiveStudentLastName))
            .ForMember(dest => dest.GradeName, opt => opt.MapFrom(src => src.AppliedGrade != null ? src.AppliedGrade.GradeName : null))
            .ForMember(dest => dest.AcademicYearName, opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null))
            .ForMember(dest => dest.ParentCount, opt => opt.MapFrom(src => src.ApplicantParents != null ? src.ApplicantParents.Count : 0))
            .ForMember(dest => dest.DocumentCount, opt => opt.MapFrom(src => src.ApplicationDocuments != null ? src.ApplicationDocuments.Count : 0))
            .ForMember(dest => dest.SubmittedDate, opt => opt.MapFrom(src => src.SubmissionDate))
            .ForMember(dest => dest.OfferExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
            .ForMember(dest => dest.IsFeePaid, opt => opt.MapFrom(src => src.ApplicationFee != null && src.ApplicationFee.Status == PaymentStatus.Completed))
            .ForMember(dest => dest.HasInterview, opt => opt.MapFrom(src => src.AdmissionInterview != null))
            .ForMember(dest => dest.HasAssessment, opt => opt.MapFrom(src => src.AdmissionAssessment != null))
            .ForMember(dest => dest.WaitlistPosition, opt => opt.MapFrom(src => src.Waitlist != null ? src.Waitlist.Position : (int?)null));

        // DTO to Entity (for create)
        CreateMap<CreateApplicationDto, Application>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ApplicationNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.ProspectiveStudentFirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.ProspectiveStudentLastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.ProspectiveStudentMiddleName, opt => opt.MapFrom(src => src.MiddleName))
            .ForMember(dest => dest.AppliedGradeId, opt => opt.MapFrom(src => src.ApplyingForGradeId))
            .ForMember(dest => dest.ApplicantParents, opt => opt.Ignore())
            .ForMember(dest => dest.ApplicationDocuments, opt => opt.Ignore())
            .ForMember(dest => dest.AcademicYear, opt => opt.Ignore())
            .ForMember(dest => dest.AppliedGrade, opt => opt.Ignore());

        // DTO to Entity (for update)
        CreateMap<UpdateApplicationDto, Application>()
            .ForMember(dest => dest.ProspectiveStudentFirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.ProspectiveStudentLastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.ProspectiveStudentMiddleName, opt => opt.MapFrom(src => src.MiddleName))
            .ForMember(dest => dest.AppliedGradeId, opt => opt.MapFrom(src => src.ApplyingForGradeId))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }

    private void CreateApplicantParentMappings()
    {
        CreateMap<ApplicantParent, ApplicantParentDto>()
            .ForMember(dest => dest.IdNumber, opt => opt.MapFrom(src => PiiMasking.Mask(src.IdNumber)));

        CreateMap<CreateApplicantParentDto, ApplicantParent>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Application, opt => opt.Ignore());

        CreateMap<UpdateApplicantParentDto, ApplicantParent>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }

    private void CreateApplicationDocumentMappings()
    {
        CreateMap<ApplicationDocument, ApplicationDocumentDto>()
            .ForMember(dest => dest.ApplicationNumber, opt => opt.MapFrom(src => src.Application != null ? src.Application.ApplicationNumber : null))
            // Don't ship the raw storage object key (held in entity.FileUrl) to
            // clients — the bucket is private and access is via signed URLs from
            // GetDownloadUrl. Leaking the internal path serves no purpose and
            // invites treating it as a clickable link again.
            .ForMember(dest => dest.FileUrl, opt => opt.Ignore())
            .ForMember(dest => dest.VerifiedByUserName, opt => opt.Ignore()); // Set in service
    }

    private void CreateApplicationFeeMappings()
    {
        CreateMap<ApplicationFee, ApplicationFeeDto>()
            .ForMember(dest => dest.ApplicationNumber, opt => opt.MapFrom(src => src.Application != null ? src.Application.ApplicationNumber : null));
    }

    private void CreateAdmissionInterviewMappings()
    {
        CreateMap<AdmissionInterview, AdmissionInterviewDto>()
            .ForMember(dest => dest.ApplicationNumber, opt => opt.MapFrom(src => src.Application != null ? src.Application.ApplicationNumber : null))
            .ForMember(dest => dest.ApplicantName, opt => opt.MapFrom(src => src.Application != null
                ? $"{src.Application.ProspectiveStudentFirstName} {src.Application.ProspectiveStudentLastName}"
                : null))
            .ForMember(dest => dest.GradeName, opt => opt.MapFrom(src => src.Application != null && src.Application.AppliedGrade != null
                ? src.Application.AppliedGrade.GradeName
                : null))
            .ForMember(dest => dest.InterviewerName, opt => opt.Ignore()); // Set in service

        CreateMap<ScheduleInterviewDto, AdmissionInterview>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Application, opt => opt.Ignore());
    }

    private void CreateAdmissionAssessmentMappings()
    {
        CreateMap<AdmissionAssessment, AdmissionAssessmentDto>()
            .ForMember(dest => dest.ApplicationNumber, opt => opt.MapFrom(src => src.Application != null ? src.Application.ApplicationNumber : null))
            .ForMember(dest => dest.ApplicantName, opt => opt.MapFrom(src => src.Application != null
                ? $"{src.Application.ProspectiveStudentFirstName} {src.Application.ProspectiveStudentLastName}"
                : null))
            .ForMember(dest => dest.GradeName, opt => opt.MapFrom(src => src.AssessedGrade != null
                ? src.AssessedGrade.GradeName
                : null))
            .ForMember(dest => dest.AssessorName, opt => opt.Ignore()); // Set in service

        CreateMap<ScheduleAssessmentDto, AdmissionAssessment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Application, opt => opt.Ignore())
            .ForMember(dest => dest.AssessedGrade, opt => opt.Ignore());
    }

    private void CreateWaitlistMappings()
    {
        CreateMap<Waitlist, WaitlistDto>()
            .ForMember(dest => dest.ApplicationNumber, opt => opt.MapFrom(src => src.Application != null ? src.Application.ApplicationNumber : null))
            .ForMember(dest => dest.ApplicantName, opt => opt.MapFrom(src => src.Application != null
                ? $"{src.Application.ProspectiveStudentFirstName} {src.Application.ProspectiveStudentLastName}"
                : null))
            .ForMember(dest => dest.GradeName, opt => opt.MapFrom(src => src.Grade != null ? src.Grade.GradeName : null));

        CreateMap<Waitlist, WaitlistPositionDto>()
            .ForMember(dest => dest.ApplicationNumber, opt => opt.MapFrom(src => src.Application != null ? src.Application.ApplicationNumber : null))
            .ForMember(dest => dest.GradeName, opt => opt.MapFrom(src => src.Grade != null ? src.Grade.GradeName : null))
            .ForMember(dest => dest.TotalInWaitlist, opt => opt.Ignore()); // Calculated in service
    }

    private void CreateAdmissionSettingsMappings()
    {
        CreateMap<Domain.Admissions.Entities.AdmissionSettings, AdmissionSettingsDto>()
            .ForMember(dest => dest.AcademicYearName, opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null))
            .ForMember(dest => dest.GradeName, opt => opt.MapFrom(src => src.Grade != null ? src.Grade.GradeName : null));

        CreateMap<CreateAdmissionSettingsDto, Domain.Admissions.Entities.AdmissionSettings>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentEnrolledCount, opt => opt.Ignore())
            .ForMember(dest => dest.AcademicYear, opt => opt.Ignore())
            .ForMember(dest => dest.Grade, opt => opt.Ignore());

        CreateMap<UpdateAdmissionSettingsDto, Domain.Admissions.Entities.AdmissionSettings>()
            .ForMember(dest => dest.CurrentEnrolledCount, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}
