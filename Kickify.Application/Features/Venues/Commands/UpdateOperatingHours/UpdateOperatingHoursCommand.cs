using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Commands.UpdateOperatingHours;

public class UpdateOperatingHoursCommand : ICommand<UpdateOperatingHoursResponse>
{
    public Guid VenueId { get; set; }
    public List<OperatingHourItemDto> OperatingHours { get; set; } = new();
}

public record OperatingHourItemDto(
    int DayOfWeek,
    string? OpenTime,
    string? CloseTime
);
