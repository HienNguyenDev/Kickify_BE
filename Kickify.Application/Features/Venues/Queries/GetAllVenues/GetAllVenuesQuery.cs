using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Venues.Queries.GetAllVenues
{
    public record GetAllVenuesQuery(
        decimal? Latitude = null,
        decimal? Longitude = null,
        double? RadiusKm = null,
        DateTime? Date = null,
        string? SportType = null,
        int Page = 1,
        int PageSize = 10
    ) : IRequest<Result<GetAllVenuesResponse>>;
}
