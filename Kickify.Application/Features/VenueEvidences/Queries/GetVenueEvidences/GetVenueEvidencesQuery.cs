using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenueEvidences.Queries.GetVenueEvidences;

public record GetVenueEvidencesQuery(Guid VenueId) : IQuery<GetVenueEvidencesResponse>;
