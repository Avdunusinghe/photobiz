namespace Photobiz.Domain.Enums
{
    /// <summary>
    /// The built-in set of gallery display layouts a tenant can choose from. A fixed platform
    /// capability rather than tenant-editable content — promote to a lookup table only if templates
    /// need to become admin-manageable without a deploy.
    /// </summary>
    public enum GalleryTemplate
    {
        Grid,
        Masonry,
        Carousel,
        Slideshow
    }
}
