using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Fields.Commands.UpdateField
{
    public record UpdateFieldCommand(
        Guid FieldId,
        string? FieldName,
        string? FieldType,
        string? SurfaceType,
        decimal? HourlyRate,
        decimal? PeakHourSurcharge,
        bool? IsActive
    ) : ICommand<UpdateFieldResponse>;
}
