using AutoMapper;
using psms.Domain.HR.Entities;
using psms.HR.StaffLeaveRequests.Dto;

namespace psms.HR.Shared;

/// <summary>
/// AutoMapper profile for the HR module.
/// Maps between domain entities and DTOs.
/// </summary>
public class HRMapper : Profile
{
    public HRMapper()
    {
        CreateStaffLeaveRequestMappings();
    }

    private void CreateStaffLeaveRequestMappings()
    {
        // Entity to DTO (full)
        CreateMap<StaffLeaveRequest, StaffLeaveRequestDto>()
            .ForMember(dest => dest.SubstituteTeacherName,
                opt => opt.MapFrom(src => src.SubstituteTeacher != null
                    ? src.SubstituteTeacher.GetFullName() : src.SubstituteTeacherName))
            .ForMember(dest => dest.TotalDays,
                opt => opt.MapFrom(src => src.TotalDays));

        // Entity to ListDto (lightweight)
        CreateMap<StaffLeaveRequest, StaffLeaveRequestListDto>()
            .ForMember(dest => dest.TotalDays,
                opt => opt.MapFrom(src => src.TotalDays));
    }
}
