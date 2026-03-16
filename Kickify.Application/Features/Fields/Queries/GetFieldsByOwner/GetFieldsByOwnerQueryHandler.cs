using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Fields.Queries.GetFieldsByOwner
{
    public class GetFieldsByOwnerQueryHandler : IQueryHandler<GetFieldsByOwnerQuery, GetFieldsByOwnerResponse>
    {
        private readonly IFieldRepository _fieldRepository;

        public GetFieldsByOwnerQueryHandler(IFieldRepository fieldRepository)
        {
            _fieldRepository = fieldRepository;
        }

        public async Task<Result<GetFieldsByOwnerResponse>> Handle(GetFieldsByOwnerQuery request, CancellationToken cancellationToken)
        {
            var (fields, total) = await _fieldRepository.GetFieldsByOwnerPagedAsync(
                request.OwnerId,
                request.Page,
                request.PageSize,
                cancellationToken
            );

            var fieldItems = fields.Select(f => new OwnerFieldItemDto(
                f.FieldId,
                f.VenueId,
                f.Venue?.VenueName ?? string.Empty,
                f.FieldName,
                f.FieldType.ToString(),
                f.SurfaceType,
                f.HourlyRate,
                f.PeakHourSurcharge,
                f.PeakStartTime,
                f.PeakEndTime,
                f.WeekendSurcharge,
                f.HolidaySurcharge,
                f.IsActive,
                f.CreatedAt
            )).ToList();

            var response = new GetFieldsByOwnerResponse(
                fieldItems,
                total,
                request.Page,
                request.PageSize
            );

            return Result.Success(response);
        }
    }
}
