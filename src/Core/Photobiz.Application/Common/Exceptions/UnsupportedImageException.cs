namespace Photobiz.Application.Common.Exceptions
{
    /// <summary>Thrown when a file claiming to be an image can't actually be decoded as one.</summary>
    public class UnsupportedImageException : Exception
    {
        public UnsupportedImageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
