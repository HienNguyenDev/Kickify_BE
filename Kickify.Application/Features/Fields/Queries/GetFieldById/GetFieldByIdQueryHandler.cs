using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Fields.Queries.GetFieldById
{
    public class GetFieldByIdQueryHandler : IQueryHandler<GetFieldByIdQuery, GetFieldByIdResponse>
    {
        private readonly IFieldRepository _fieldRepository;

        public GetFieldByIdQueryHandler(IFieldRepository fieldRepository)
        {
            _fieldRepository = fieldRepository;
        }

        public async Task<Result<GetFieldByIdResponse>> Handle(GetFieldByIdQuery request, CancellationToken cancellationToken)
        {
            var field = await _fieldRepository.GetFieldWithVenueAsync(request.FieldId, cancellationToken);

            if (field == null)
            {
                return Result.Failure<GetFieldByIdResponse>(FieldErrors.NotFound(request.FieldId));
            }

            var operatingHours = field.Venue?.VenueOperatingHours?
                .Select(oh => new OperatingHourDto(
                    (DayOfWeek)oh.DayOfWeek,
                    oh.OpenTime ?? TimeSpan.Zero,
                    oh.CloseTime ?? TimeSpan.Zero,
                    oh.IsClosed
                ))
                .OrderBy(oh => oh.DayOfWeek)
                .ToList() ?? new List<OperatingHourDto>();

            var response = new GetFieldByIdResponse(
                field.FieldId,
                field.VenueId,
                field.Venue?.VenueName ?? string.Empty,
                field.Venue?.Address ?? string.Empty,
                field.FieldName,
                field.FieldType.ToString(),
                field.SurfaceType,
                field.HourlyRate,
                field.PeakHourSurcharge,
                field.PeakStartTime,
                field.PeakEndTime,
                field.WeekendSurcharge,
                field.HolidaySurcharge,
                field.IsActive,
                operatingHours,
                field.CreatedAt
            );

            return Result.Success(response);
        }
    }
}
