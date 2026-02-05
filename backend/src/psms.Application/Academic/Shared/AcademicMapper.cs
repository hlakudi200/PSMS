using AutoMapper;
using psms.Academic.AcademicYears.Dto;
using psms.Academic.Grades.Dto;
using psms.Academic.GradeSubjects.Dto;
using psms.Academic.Subjects.Dto;
using psms.Academic.Terms.Dto;
using psms.Domain.Academic.Entities;
using System.Linq;

namespace psms.Academic.Shared;

/// <summary>
/// AutoMapper profile for the Academic module.
/// Maps between domain entities and DTOs.
/// </summary>
public class AcademicMapper : Profile
{
    public AcademicMapper()
    {
        CreateGradeMappings();
        CreateAcademicYearMappings();
        CreateTermMappings();
        CreateSubjectMappings();
        CreateGradeSubjectMappings();
    }

    private void CreateGradeMappings()
    {
        // Entity to DTO (full)
        CreateMap<Grade, GradeDto>()
            .ForMember(dest => dest.ClassCount,
                opt => opt.MapFrom(src => src.Classes != null ? src.Classes.Count(c => !c.IsDeleted) : 0))
            .ForMember(dest => dest.StudentCount,
                opt => opt.MapFrom(src => src.Students != null ? src.Students.Count(s => !s.IsDeleted) : 0))
            .ForMember(dest => dest.SubjectCount,
                opt => opt.MapFrom(src => src.GradeSubjects != null ? src.GradeSubjects.Count : 0));

        // Entity to ListDto (lightweight)
        CreateMap<Grade, GradeListDto>()
            .ForMember(dest => dest.ClassCount,
                opt => opt.MapFrom(src => src.Classes != null ? src.Classes.Count(c => !c.IsDeleted) : 0))
            .ForMember(dest => dest.StudentCount,
                opt => opt.MapFrom(src => src.Students != null ? src.Students.Count(s => !s.IsDeleted) : 0));

        // CreateDto to Entity
        CreateMap<CreateGradeDto, Grade>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Classes, opt => opt.Ignore())
            .ForMember(dest => dest.Students, opt => opt.Ignore())
            .ForMember(dest => dest.GradeSubjects, opt => opt.Ignore());

        // UpdateDto to Entity (null-skip)
        CreateMap<UpdateGradeDto, Grade>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }

    private void CreateAcademicYearMappings()
    {
        // Entity to DTO
        CreateMap<AcademicYear, AcademicYearDto>()
            .ForMember(dest => dest.TermCount,
                opt => opt.MapFrom(src => src.Terms != null ? src.Terms.Count : 0))
            .ForMember(dest => dest.ClassCount,
                opt => opt.MapFrom(src => src.Classes != null ? src.Classes.Count(c => !c.IsDeleted) : 0))
            .ForMember(dest => dest.Terms, opt => opt.Ignore()); // Mapped manually in service

        // Entity to ListDto
        CreateMap<AcademicYear, AcademicYearListDto>()
            .ForMember(dest => dest.TermCount,
                opt => opt.MapFrom(src => src.Terms != null ? src.Terms.Count : 0))
            .ForMember(dest => dest.ClassCount,
                opt => opt.MapFrom(src => src.Classes != null ? src.Classes.Count(c => !c.IsDeleted) : 0));
    }

    private void CreateTermMappings()
    {
        // Entity to DTO
        CreateMap<Term, TermDto>()
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));

        // Entity to ListDto
        CreateMap<Term, TermListDto>()
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));
    }

    private void CreateSubjectMappings()
    {
        // Entity to DTO (full)
        CreateMap<Subject, SubjectDto>()
            .ForMember(dest => dest.GradeCount,
                opt => opt.MapFrom(src => src.GradeSubjects != null ? src.GradeSubjects.Count : 0))
            .ForMember(dest => dest.TeacherCount,
                opt => opt.MapFrom(src => src.TeacherSubjects != null ? src.TeacherSubjects.Count : 0));

        // Entity to ListDto (lightweight)
        CreateMap<Subject, SubjectListDto>()
            .ForMember(dest => dest.GradeCount,
                opt => opt.MapFrom(src => src.GradeSubjects != null ? src.GradeSubjects.Count : 0));
    }

    private void CreateGradeSubjectMappings()
    {
        // Entity to DTO with flattened navigation properties
        CreateMap<GradeSubject, GradeSubjectDto>()
            .ForMember(dest => dest.GradeName,
                opt => opt.MapFrom(src => src.Grade != null ? src.Grade.GradeName : null))
            .ForMember(dest => dest.GradeLevel,
                opt => opt.MapFrom(src => src.Grade != null ? src.Grade.GradeLevel : default))
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectName : null))
            .ForMember(dest => dest.SubjectCode,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectCode : null))
            .ForMember(dest => dest.IsCore,
                opt => opt.MapFrom(src => src.Subject != null && src.Subject.IsCore));
    }
}
