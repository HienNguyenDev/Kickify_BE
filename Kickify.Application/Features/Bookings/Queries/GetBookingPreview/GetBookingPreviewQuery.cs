using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Bookings.Queries.GetBookingPreview
{
    public record GetBookingPreviewQuery(
        Guid FieldId,
        DateTime Date,
        TimeSpan StartTime,
        TimeSpan EndTime,
        int NumberOfPlayers
    ) : IRequest<Result<GetBookingPreviewResponse>>;
}
