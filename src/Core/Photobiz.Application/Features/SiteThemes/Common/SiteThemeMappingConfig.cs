using Mapster;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Features.SiteThemes.Common
{
    /// <summary>Mapster registration for <see cref="SiteTheme"/> projections, picked up by <c>TypeAdapterConfig.GlobalSettings.Scan(...)</c>.</summary>
    public class SiteThemeMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<SiteTheme, SiteThemeDto>()
                .Map(dest => dest.FooterLinks, src => src.FooterLinks.OrderBy(link => link.DisplayOrder));
        }
    }
}
