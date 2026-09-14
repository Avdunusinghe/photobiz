namespace Photobiz.Application.Common.Interfaces
{
    public record ProcessedImage(byte[] Content, string ContentType, string FileExtension, int Width, int Height);

    /// <summary>
    /// Normalizes an uploaded raster image for the web: re-encodes it to a modern, small format and
    /// caps its dimensions. Both matter for SEO — Core Web Vitals (page-speed) is a ranking factor,
    /// and a smaller, correctly-sized image loads faster than whatever the tenant happened to upload.
    /// </summary>
    public interface IImageProcessor
    {
        /// <summary>
        /// Decodes <paramref name="content"/>, downscales it to fit within
        /// <paramref name="maxDimension"/> on its longest side (never upscales), and re-encodes it as
        /// WebP. Throws <see cref="Exceptions.UnsupportedImageException"/> if the stream isn't a
        /// decodable raster image at all.
        /// </summary>
        Task<ProcessedImage> ConvertToWebPAsync(
            Stream content,
            int maxDimension,
            CancellationToken cancellationToken = default);
    }
}
