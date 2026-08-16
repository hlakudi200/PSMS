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
            .ForMember(dest => dest.IsConfigured, opt => opt.MapFrom(_ => true))
            // Raw stored value; SchoolName is then resolved to the fallback by
            // the app service. Mapped explicitly rather than by convention so
            // the pair stays obvious.
            .ForMember(dest => dest.ConfiguredSchoolName,
                opt => opt.MapFrom(src => src.SchoolName));

        CreateMap<SchoolBranding, PublicSchoolBrandingDto>();
    }
}
