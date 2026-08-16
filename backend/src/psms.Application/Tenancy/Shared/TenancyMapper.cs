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
            // IsConfigured is set by the app service, which compares the row
            // against the defaults. It cannot mean "a row exists": tenant
            // provisioning now seeds a defaults-only row for every new school,
            // so that reading would be true for everyone and the "using default
            // branding" hint would never appear.
            .ForMember(dest => dest.IsConfigured, opt => opt.Ignore())
            // Raw stored value; SchoolName is then resolved to the fallback by
            // the app service. Mapped explicitly rather than by convention so
            // the pair stays obvious.
            .ForMember(dest => dest.ConfiguredSchoolName,
                opt => opt.MapFrom(src => src.SchoolName));

        CreateMap<SchoolBranding, PublicSchoolBrandingDto>();
    }
}
