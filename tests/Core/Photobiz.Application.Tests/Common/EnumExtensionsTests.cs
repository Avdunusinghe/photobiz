using Photobiz.Domain.Common;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Tests.Common
{
    public class EnumExtensionsTests
    {
        [Theory]
        [InlineData(BookingStatus.Pending, "Booking requested, awaiting confirmation.")]
        [InlineData(BookingStatus.Confirmed, "Booking confirmed with the client.")]
        [InlineData(BookingStatus.Paid, "Payment received in full.")]
        [InlineData(BookingStatus.Cancelled, "Booking cancelled.")]
        public void GetDescription_ReturnsAttributeValue(BookingStatus status, string expectedDescription)
        {
            Assert.Equal(expectedDescription, status.GetDescription());
        }
    }
}
