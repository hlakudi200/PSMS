using AutoMapper;
using psms.Domain.Learning.Entities;
using psms.Learning.LearningMaterials.Dto;
using psms.Learning.OnlineLessons.Dto;

namespace psms.Learning.Shared;

/// <summary>
/// AutoMapper profile for the Learning module.
/// Maps between domain entities and DTOs.
/// </summary>
public class LearningMapper : Profile
{
    public LearningMapper()
    {
        CreateLearningMaterialMappings();
        CreateOnlineLessonMappings();
    }

    private void CreateLearningMaterialMappings()
    {
        // Entity to DTO (full)
        CreateMap<LearningMaterial, LearningMaterialDto>()
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.ClassSubject != null && src.ClassSubject.Class != null
                    ? src.ClassSubject.Class.ClassName : null))
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.ClassSubject != null && src.ClassSubject.Subject != null
                    ? src.ClassSubject.Subject.SubjectName : null))
            .ForMember(dest => dest.TermName,
                opt => opt.MapFrom(src => src.Term != null ? src.Term.TermName : null));

        // Entity to ListDto (lightweight)
        CreateMap<LearningMaterial, LearningMaterialListDto>();

        // Version row -> DTO (1:1, convention covers everything).
        CreateMap<LearningMaterialVersion, LearningMaterialVersionDto>();
    }

    private void CreateOnlineLessonMappings()
    {
        // Entity to DTO (full)
        CreateMap<OnlineLesson, OnlineLessonDto>()
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.ClassSubject != null && src.ClassSubject.Class != null
                    ? src.ClassSubject.Class.ClassName : null))
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.ClassSubject != null && src.ClassSubject.Subject != null
                    ? src.ClassSubject.Subject.SubjectName : null));

        // Entity to ListDto (lightweight)
        CreateMap<OnlineLesson, OnlineLessonListDto>()
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.ClassSubject != null && src.ClassSubject.Class != null
                    ? src.ClassSubject.Class.ClassName : null))
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.ClassSubject != null && src.ClassSubject.Subject != null
                    ? src.ClassSubject.Subject.SubjectName : null));
    }
}
