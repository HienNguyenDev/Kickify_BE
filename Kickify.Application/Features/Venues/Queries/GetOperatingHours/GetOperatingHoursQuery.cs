using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Queries.GetOperatingHours;

public class GetOperatingHoursQuery : IQuery<GetOperatingHoursResponse>
{
    public Guid VenueId { get; set; }
}
