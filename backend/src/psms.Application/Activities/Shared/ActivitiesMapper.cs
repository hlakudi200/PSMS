using AutoMapper;
using psms.Activities.FieldTrips.Dto;
using psms.Domain.Activities.Entities;

namespace psms.Activities.Shared;

/// <summary>
/// AutoMapper profile for the Activities module.
/// Maps between domain entities and DTOs.
/// </summary>
public class ActivitiesMapper : Profile
{
    public ActivitiesMapper()
    {
        CreateFieldTripMappings();
    }

    private void CreateFieldTripMappings()
    {
        // Entity to DTO (full)
        CreateMap<FieldTrip, FieldTripDto>()
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null))
            .ForMember(dest => dest.TermName,
                opt => opt.MapFrom(src => src.Term != null ? src.Term.TermName : null))
            .ForMember(dest => dest.OrganizingTeacherName,
                opt => opt.MapFrom(src => src.OrganizingTeacher != null ? src.OrganizingTeacher.GetFullName() : null))
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : null))
            .ForMember(dest => dest.GradeName,
                opt => opt.MapFrom(src => src.Grade != null ? src.Grade.GradeName : null));

        // Entity to ListDto (lightweight)
        CreateMap<FieldTrip, FieldTripListDto>()
            .ForMember(dest => dest.OrganizingTeacherName,
                opt => opt.MapFrom(src => src.OrganizingTeacher != null ? src.OrganizingTeacher.GetFullName() : null));
    }
}
