using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using MediatR;

namespace Kickify.Application.Features.Bookings.Queries.GetBookingPreview
{
    public class GetBookingPreviewQueryHandler : IRequestHandler<GetBookingPreviewQuery, Result<GetBookingPreviewResponse>>
    {
        private readonly IFieldRepository _fieldRepository;

        public GetBookingPreviewQueryHandler(IFieldRepository fieldRepository)
        {
            _fieldRepository = fieldRepository;
        }

        public async Task<Result<GetBookingPreviewResponse>> Handle(GetBookingPreviewQuery request, CancellationToken cancellationToken)
        {
            // Get field with venue
            var field = await _fieldRepository.GetFieldWithVenueAsync(request.FieldId, cancellationToken);
            if (field == null)
            {
                return Result.Failure<GetBookingPreviewResponse>(FieldErrors.NotFound(request.FieldId));
            }

            // Validate time slot
            if (request.StartTime >= request.EndTime)
            {
                return Result.Failure<GetBookingPreviewResponse>(
                    new Error("Booking.InvalidTimeSlot", "Start time must be before end time", ErrorType.Validation));
            }

            // Validate number of players (we don't have MaxPlayers in entity, so skip this check)

            // Check if field is available for this time slot
            var isAvailable = await _fieldRepository.IsFieldAvailableAsync(
                request.FieldId,
                request.Date,
                request.StartTime,
                request.EndTime,
                cancellationToken);

            if (!isAvailable)
            {
                return Result.Failure<GetBookingPreviewResponse>(FieldErrors.NotAvailable);
            }

            // Calculate duration in hours
            var duration = request.EndTime - request.StartTime;
            var durationHours = (decimal)duration.TotalHours;

            // Calculate total amount
            var totalAmount = field.HourlyRate * durationHours;

            // Calculate share per player
            var sharePerPlayer = totalAmount / request.NumberOfPlayers;

            return Result.Success(new GetBookingPreviewResponse(
                field.FieldId,
                field.FieldName,
                field.Venue.VenueName,
                request.Date,
                request.StartTime,
                request.EndTime,
                durationHours,
                field.HourlyRate,
                totalAmount,
                request.NumberOfPlayers,
                sharePerPlayer,
                isAvailable
            ));
        }
    }
}
