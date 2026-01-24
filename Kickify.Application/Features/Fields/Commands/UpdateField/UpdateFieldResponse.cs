namespace Kickify.Application.Features.Fields.Commands.UpdateField
{
    public record UpdateFieldResponse(
        Guid FieldId,
        Guid VenueId,
        string FieldName,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal PeakHourSurcharge,
        bool IsActive,
        DateTime UpdatedAt
    );
}
