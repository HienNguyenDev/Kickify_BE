using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Fields.Queries.GetAllFields
{
    public class GetAllFieldsQueryHandler : IQueryHandler<GetAllFieldsQuery, GetAllFieldsResponse>
    {
        private readonly IFieldRepository _fieldRepository;

        public GetAllFieldsQueryHandler(IFieldRepository fieldRepository)
        {
            _fieldRepository = fieldRepository;
        }

        public async Task<Result<GetAllFieldsResponse>> Handle(GetAllFieldsQuery request, CancellationToken cancellationToken)
        {
            FieldType? fieldType = null;
            if (!string.IsNullOrEmpty(request.FieldType))
            {
                if (Enum.TryParse<FieldType>(request.FieldType, true, out var parsed))
                {
                    fieldType = parsed;
                }
            }

            var (fields, total) = await _fieldRepository.GetFieldsPagedAsync(
                fieldType,
                request.IsActive,
                request.Page,
                request.PageSize,
                cancellationToken
            );

            var fieldItems = fields.Select(f => new FieldItemDto(
                f.FieldId,
                f.VenueId,
                f.Venue?.VenueName ?? string.Empty,
                f.Venue?.Address ?? string.Empty,
                f.FieldName,
                f.FieldType.ToString(),
                f.SurfaceType,
                f.HourlyRate,
                f.PeakHourSurcharge,
                f.IsActive,
                f.CreatedAt
            )).ToList();

            var response = new GetAllFieldsResponse(
                fieldItems,
                total,
                request.Page,
                request.PageSize,
                (int)Math.Ceiling(total / (double)request.PageSize)
            );

            return Result.Success(response);
        }
    }
}
