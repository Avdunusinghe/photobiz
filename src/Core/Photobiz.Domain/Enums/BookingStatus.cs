using System.ComponentModel;

namespace Photobiz.Domain.Enums
{
    public enum BookingStatus
    {
        [Description("Booking requested, awaiting confirmation.")]
        Pending,

        [Description("Booking confirmed with the client.")]
        Confirmed,

        [Description("Payment received in full.")]
        Paid,

        [Description("Booking cancelled.")]
        Cancelled
    }
}
