using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Fields.Commands.UpdateField
{
    public record UpdateFieldCommand(
        Guid FieldId,
        Guid UserId,
        string? FieldName,
        string? FieldType,
        string? SurfaceType,
        decimal? HourlyRate,
        decimal? PeakHourSurcharge,
        bool? IsActive
    ) : IRequest<Result<UpdateFieldResponse>>;
}
