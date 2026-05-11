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
                f.WeekendSurcharge,
                f.HolidaySurcharge,
                f.IsActive,
                f.CreatedAt,
                f.PeakHours.Select(ph => new OwnerFieldPeakHourResponseDto(
                    ph.Id,
                    ph.StartTime,
                    ph.EndTime,
                    ph.SurchargeAmount,
                    ph.IsPercentage,
                    ph.ApplicableDays
                )).ToList(),
                f.IsWeekendSurchargePercentage,
                f.IsHolidaySurchargePercentage
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
