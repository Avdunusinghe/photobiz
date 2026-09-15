namespace Photobiz.PortfolioApp.Models
{
    public record GalleryDetailsViewModel(PublicSite Site, PublicGallery Gallery)
    {
        /// <summary>The gallery's own template choice, falling back to the tenant's site-wide default.</summary>
        public GalleryTemplate EffectiveTemplate => Gallery.Template ?? Site.DefaultGalleryTemplate;
    }
}
