using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Bookings.Queries.GetBookingPreview
{
    public record GetBookingPreviewQuery(
        Guid FieldId,
        DateTime Date,
        TimeSpan StartTime,
        int DurationMinutes,
        int NumberOfPlayers
    ) : IQuery<GetBookingPreviewResponse>;
}
