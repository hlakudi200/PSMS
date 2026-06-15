using AutoMapper;
using psms.Assessment.Assessments.Dto;
using psms.Assessment.AssessmentQuestions.Dto;
using psms.Assessment.Marks.Dto;
using psms.Assessment.Reports.Dto;
using psms.Assessment.ReportSubjects.Dto;
using psms.Domain.Assessment.Entities;
using System;
using System.Linq;
using AssessmentEntity = psms.Domain.Assessment.Entities.Assessment;

namespace psms.Assessment.Shared;

/// <summary>
/// AutoMapper profile for the Assessment module.
/// Maps between domain entities and DTOs.
/// </summary>
public class AssessmentMapper : Profile
{
    public AssessmentMapper()
    {
        CreateAssessmentMappings();
        CreateAssessmentQuestionMappings();
        CreateMarkMappings();
        CreateReportMappings();
        CreateReportSubjectMappings();
    }

    private void CreateAssessmentMappings()
    {
        // Entity to DTO (full)
        CreateMap<AssessmentEntity, AssessmentDto>()
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.ClassSubject != null && src.ClassSubject.Class != null
                    ? src.ClassSubject.Class.ClassName : null))
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.ClassSubject != null && src.ClassSubject.Subject != null
                    ? src.ClassSubject.Subject.SubjectName : null))
            .ForMember(dest => dest.TermName,
                opt => opt.MapFrom(src => src.Term != null ? src.Term.TermName : null))
            .ForMember(dest => dest.MarkCount,
                opt => opt.MapFrom(src => src.Marks != null ? src.Marks.Count : 0))
            .ForMember(dest => dest.QuestionCount,
                opt => opt.MapFrom(src => src.Questions != null ? src.Questions.Count : 0));

        // Entity to ListDto (lightweight)
        CreateMap<AssessmentEntity, AssessmentListDto>()
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.ClassSubject != null && src.ClassSubject.Class != null
                    ? src.ClassSubject.Class.ClassName : null))
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.ClassSubject != null && src.ClassSubject.Subject != null
                    ? src.ClassSubject.Subject.SubjectName : null))
            .ForMember(dest => dest.TermName,
                opt => opt.MapFrom(src => src.Term != null ? src.Term.TermName : null))
            .ForMember(dest => dest.MarkCount,
                opt => opt.MapFrom(src => src.Marks != null ? src.Marks.Count : 0))
            .ForMember(dest => dest.QuestionCount,
                opt => opt.MapFrom(src => src.Questions != null ? src.Questions.Count : 0));
    }

    private void CreateAssessmentQuestionMappings()
    {
        // Entity to DTO (full)
        CreateMap<AssessmentQuestion, AssessmentQuestionDto>();

        // Entity to ListDto (lightweight)
        CreateMap<AssessmentQuestion, AssessmentQuestionListDto>();
    }

    private void CreateMarkMappings()
    {
        // Entity to DTO (full)
        CreateMap<Mark, MarkDto>()
            .ForMember(dest => dest.AssessmentName,
                opt => opt.MapFrom(src => src.Assessment != null ? src.Assessment.Name : null))
            .ForMember(dest => dest.AssessmentMaxMarks,
                opt => opt.MapFrom(src => src.Assessment != null ? src.Assessment.MaxMarks : 0))
            .ForMember(dest => dest.MarksReleased,
                opt => opt.MapFrom(src => src.Assessment != null && src.Assessment.MarksReleased))
            .ForMember(dest => dest.FeedbackEditableUntil,
                opt => opt.MapFrom(src =>
                    src.Assessment != null && src.Assessment.MarksReleased && src.Assessment.MarksReleasedDate.HasValue
                        ? (DateTime?)src.Assessment.MarksReleasedDate.Value.AddHours(48)
                        : null))
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null));

        // Entity to ListDto (lightweight)
        CreateMap<Mark, MarkListDto>()
            .ForMember(dest => dest.AssessmentName,
                opt => opt.MapFrom(src => src.Assessment != null ? src.Assessment.Name : null))
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null));
    }

    private void CreateReportMappings()
    {
        // Entity to DTO (full) — SubjectReports mapped manually in service
        CreateMap<Report, ReportDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : null))
            .ForMember(dest => dest.TermName,
                opt => opt.MapFrom(src => src.Term != null ? src.Term.TermName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null))
            .ForMember(dest => dest.PromotedToGradeName,
                opt => opt.MapFrom(src => src.PromotedToGrade != null ? src.PromotedToGrade.GradeName : null))
            .ForMember(dest => dest.SubjectReports, opt => opt.Ignore());

        // Entity to ListDto (lightweight)
        CreateMap<Report, ReportListDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : null))
            .ForMember(dest => dest.TermName,
                opt => opt.MapFrom(src => src.Term != null ? src.Term.TermName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null))
            .ForMember(dest => dest.SubjectCount,
                opt => opt.MapFrom(src => src.SubjectReports != null ? src.SubjectReports.Count : 0));
    }

    private void CreateReportSubjectMappings()
    {
        CreateMap<ReportSubject, ReportSubjectDto>()
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectName : null))
            .ForMember(dest => dest.SubjectCode,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectCode : null))
            .ForMember(dest => dest.TeacherName,
                opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.GetFullName() : null));
    }
}
