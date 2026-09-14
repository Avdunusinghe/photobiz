using Photobiz.Application.Common.Exceptions;
using Photobiz.Infrastructure.Media;
using SkiaSharp;

namespace Photobiz.Infrastructure.Tests.Media
{
    public class SkiaImageProcessorTests
    {
        private readonly SkiaImageProcessor _processor = new();

        private static MemoryStream MakePng(int width, int height)
        {
            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.CornflowerBlue);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return new MemoryStream(data.ToArray());
        }

        [Fact]
        public async Task ConvertToWebPAsync_ReturnsWebPContent()
        {
            using var source = MakePng(200, 200);

            var result = await _processor.ConvertToWebPAsync(source, maxDimension: 512);

            Assert.Equal("image/webp", result.ContentType);
            Assert.Equal("webp", result.FileExtension);
            Assert.NotEmpty(result.Content);
        }

        [Fact]
        public async Task ConvertToWebPAsync_WhenSmallerThanTheMax_DoesNotUpscale()
        {
            using var source = MakePng(100, 80);

            var result = await _processor.ConvertToWebPAsync(source, maxDimension: 512);

            Assert.Equal(100, result.Width);
            Assert.Equal(80, result.Height);
        }

        [Fact]
        public async Task ConvertToWebPAsync_WhenLargerThanTheMax_DownscalesPreservingAspectRatio()
        {
            using var source = MakePng(1000, 500);

            var result = await _processor.ConvertToWebPAsync(source, maxDimension: 512);

            Assert.Equal(512, result.Width);
            Assert.Equal(256, result.Height);
        }

        [Fact]
        public async Task ConvertToWebPAsync_WhenTallerThanWide_ScalesByTheTallerDimension()
        {
            using var source = MakePng(500, 1000);

            var result = await _processor.ConvertToWebPAsync(source, maxDimension: 512);

            Assert.Equal(256, result.Width);
            Assert.Equal(512, result.Height);
        }

        [Fact]
        public async Task ConvertToWebPAsync_WithCorruptContent_ThrowsUnsupportedImageException()
        {
            using var garbage = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8]);

            await Assert.ThrowsAsync<UnsupportedImageException>(() =>
                _processor.ConvertToWebPAsync(garbage, maxDimension: 512));
        }

        [Fact]
        public async Task ConvertToWebPAsync_WithEmptyContent_ThrowsUnsupportedImageException()
        {
            using var empty = new MemoryStream([]);

            await Assert.ThrowsAsync<UnsupportedImageException>(() =>
                _processor.ConvertToWebPAsync(empty, maxDimension: 512));
        }
    }
}
