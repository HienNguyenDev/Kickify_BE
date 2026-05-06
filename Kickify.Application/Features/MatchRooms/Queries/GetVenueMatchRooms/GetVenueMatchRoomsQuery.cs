using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Queries.GetVenueMatchRooms;

public record GetVenueMatchRoomsQuery(
    Guid? VenueId,
    Guid? FieldId,
    DateTime? Date,
    string? Status,
    int Page = 1,
    int PageSize = 10
) : IQuery<GetVenueMatchRoomsResponse>;
