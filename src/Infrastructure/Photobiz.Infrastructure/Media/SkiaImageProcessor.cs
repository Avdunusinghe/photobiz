using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using SkiaSharp;

namespace Photobiz.Infrastructure.Media
{
    /// <summary>
    /// Real image decoding/resizing/re-encoding via SkiaSharp (MIT-licensed, no revenue-based
    /// commercial licensing requirement — unlike SixLabors.ImageSharp v3+).
    /// </summary>
    public sealed class SkiaImageProcessor : IImageProcessor
    {
        private const int WebpQuality = 80;

        public Task<ProcessedImage> ConvertToWebPAsync(
            Stream content,
            int maxDimension,
            CancellationToken cancellationToken = default)
        {
            using var original = Decode(content);
            using var resized = Fit(original, maxDimension);

            using var image = SKImage.FromBitmap(resized);
            using var encoded = image.Encode(SKEncodedImageFormat.Webp, WebpQuality)
                ?? throw new UnsupportedImageException(
                    "The uploaded file could not be re-encoded.", new InvalidOperationException());

            return Task.FromResult(new ProcessedImage(
                encoded.ToArray(), "image/webp", "webp", resized.Width, resized.Height));
        }

        private static SKBitmap Decode(Stream content)
        {
            SKBitmap? bitmap;
            try
            {
                bitmap = SKBitmap.Decode(content);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new UnsupportedImageException("The uploaded file is not a supported image.", ex);
            }

            return bitmap ?? throw new UnsupportedImageException(
                "The uploaded file is not a supported image.", new InvalidDataException());
        }

        /// <summary>Downscales to fit within a maxDimension × maxDimension box, preserving aspect ratio; never upscales.</summary>
        private static SKBitmap Fit(SKBitmap source, int maxDimension)
        {
            if (source.Width <= maxDimension && source.Height <= maxDimension)
            {
                return source.Copy();
            }

            var scale = (double)maxDimension / Math.Max(source.Width, source.Height);
            var width = Math.Max(1, (int)Math.Round(source.Width * scale));
            var height = Math.Max(1, (int)Math.Round(source.Height * scale));

            var resized = source.Resize(new SKImageInfo(width, height), SKSamplingOptions.Default);

            return resized ?? throw new UnsupportedImageException(
                "The uploaded file could not be resized.", new InvalidOperationException());
        }
    }
}
