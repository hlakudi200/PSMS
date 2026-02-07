using AutoMapper;
using psms.Domain.SASpecific.Entities;
using psms.SASpecific.AfterCares.Dto;
using psms.SASpecific.ExtramuralActivities.Dto;
using psms.SASpecific.SchoolTransports.Dto;
using psms.SASpecific.StudentAfterCares.Dto;
using psms.SASpecific.StudentExtramurals.Dto;
using psms.SASpecific.StudentTransports.Dto;

namespace psms.SASpecific.Shared;

/// <summary>
/// AutoMapper profile for the SASpecific module.
/// Maps between domain entities and DTOs.
/// </summary>
public class SASpecificMapper : Profile
{
    public SASpecificMapper()
    {
        CreateAfterCareMappings();
        CreateStudentAfterCareMappings();
        CreateExtramuralActivityMappings();
        CreateStudentExtramuralMappings();
        CreateSchoolTransportMappings();
        CreateStudentTransportMappings();
    }

    private void CreateAfterCareMappings()
    {
        CreateMap<AfterCare, AfterCareDto>()
            .ForMember(dest => dest.EnrollmentCount,
                opt => opt.MapFrom(src => src.StudentEnrollments != null ? src.StudentEnrollments.Count : 0));

        CreateMap<AfterCare, AfterCareListDto>()
            .ForMember(dest => dest.EnrollmentCount,
                opt => opt.MapFrom(src => src.StudentEnrollments != null ? src.StudentEnrollments.Count : 0));
    }

    private void CreateStudentAfterCareMappings()
    {
        CreateMap<StudentAfterCare, StudentAfterCareDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? src.Student.FirstName + " " + src.Student.LastName : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.AfterCareProgramName,
                opt => opt.MapFrom(src => src.AfterCare != null ? src.AfterCare.ProgramName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));

        CreateMap<StudentAfterCare, StudentAfterCareListDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? src.Student.FirstName + " " + src.Student.LastName : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.AfterCareProgramName,
                opt => opt.MapFrom(src => src.AfterCare != null ? src.AfterCare.ProgramName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));
    }

    private void CreateExtramuralActivityMappings()
    {
        CreateMap<ExtramuralActivity, ExtramuralActivityDto>()
            .ForMember(dest => dest.EnrollmentCount,
                opt => opt.MapFrom(src => src.StudentEnrollments != null ? src.StudentEnrollments.Count : 0));

        CreateMap<ExtramuralActivity, ExtramuralActivityListDto>()
            .ForMember(dest => dest.EnrollmentCount,
                opt => opt.MapFrom(src => src.StudentEnrollments != null ? src.StudentEnrollments.Count : 0));
    }

    private void CreateStudentExtramuralMappings()
    {
        CreateMap<StudentExtramural, StudentExtramuralDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? src.Student.FirstName + " " + src.Student.LastName : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.ActivityName,
                opt => opt.MapFrom(src => src.ExtramuralActivity != null ? src.ExtramuralActivity.ActivityName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));

        CreateMap<StudentExtramural, StudentExtramuralListDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? src.Student.FirstName + " " + src.Student.LastName : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.ActivityName,
                opt => opt.MapFrom(src => src.ExtramuralActivity != null ? src.ExtramuralActivity.ActivityName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));
    }

    private void CreateSchoolTransportMappings()
    {
        CreateMap<SchoolTransport, SchoolTransportDto>()
            .ForMember(dest => dest.EnrollmentCount,
                opt => opt.MapFrom(src => src.StudentEnrollments != null ? src.StudentEnrollments.Count : 0));

        CreateMap<SchoolTransport, SchoolTransportListDto>()
            .ForMember(dest => dest.EnrollmentCount,
                opt => opt.MapFrom(src => src.StudentEnrollments != null ? src.StudentEnrollments.Count : 0));
    }

    private void CreateStudentTransportMappings()
    {
        CreateMap<StudentTransport, StudentTransportDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? src.Student.FirstName + " " + src.Student.LastName : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.RouteName,
                opt => opt.MapFrom(src => src.SchoolTransport != null ? src.SchoolTransport.RouteName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));

        CreateMap<StudentTransport, StudentTransportListDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? src.Student.FirstName + " " + src.Student.LastName : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.RouteName,
                opt => opt.MapFrom(src => src.SchoolTransport != null ? src.SchoolTransport.RouteName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));
    }
}
