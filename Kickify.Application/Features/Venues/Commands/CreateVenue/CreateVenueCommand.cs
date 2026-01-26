using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Venues.Commands.CreateVenue
{
    public record CreateVenueCommand(
        Guid OwnerId,
        string Name,
        string Address,
        decimal Latitude,
        decimal Longitude,
        string? ContactPhone,
        string? ContactEmail,
        string? Description,
        string? Amenities,
        List<CreateVenueFieldDto> Fields,
        List<CreateVenueOperatingHoursDto> OperatingHours
    ) : IRequest<Result<CreateVenueResponse>>;

    public record CreateVenueFieldDto(
        string Name,
        string FieldType,
        int MaxPlayers,
        decimal PricePerHour,
        string? Description
    );

    public record CreateVenueOperatingHoursDto(
        DayOfWeek DayOfWeek,
        TimeSpan OpenTime,
        TimeSpan CloseTime
    );
}
