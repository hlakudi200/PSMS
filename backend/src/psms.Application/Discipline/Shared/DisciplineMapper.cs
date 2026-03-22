using AutoMapper;
using psms.Discipline.DisciplinaryCases.Dto;
using psms.Domain.Discipline.Entities;

namespace psms.Discipline.Shared;

/// <summary>
/// AutoMapper profile for the Discipline module.
/// Maps between domain entities and DTOs.
/// </summary>
public class DisciplineMapper : Profile
{
    public DisciplineMapper()
    {
        CreateDisciplinaryCaseMappings();
    }

    private void CreateDisciplinaryCaseMappings()
    {
        // Entity to DTO (full)
        CreateMap<DisciplinaryCase, DisciplinaryCaseDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? src.Student.FirstName + " " + src.Student.LastName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));

        // Entity to ListDto (lightweight)
        CreateMap<DisciplinaryCase, DisciplinaryCaseListDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? src.Student.FirstName + " " + src.Student.LastName : null));
    }
}
