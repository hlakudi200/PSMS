using AutoMapper;
using psms.Domain.Tenancy.Entities;
using psms.Tenancy.Branding.Dto;

namespace psms.Tenancy.Shared;

public class TenancyMapper : Profile
{
    public TenancyMapper()
    {
        CreateSchoolBrandingMappings();
    }

    private void CreateSchoolBrandingMappings()
    {
        CreateMap<SchoolBranding, SchoolBrandingDto>()
            .ForMember(dest => dest.IsConfigured, opt => opt.MapFrom(_ => true));

        CreateMap<SchoolBranding, PublicSchoolBrandingDto>();
    }
}
