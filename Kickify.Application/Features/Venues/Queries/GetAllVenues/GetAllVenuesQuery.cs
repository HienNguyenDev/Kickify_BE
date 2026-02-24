using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Queries.GetAllVenues
{
    public record GetAllVenuesQuery(
        decimal? Latitude = null,
        decimal? Longitude = null,
        double? RadiusKm = null,
        DateTime? Date = null,
        string? FieldType = null,
        string? SearchName = null,
        string? Status = null,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<GetAllVenuesResponse>;
}
