using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.Venues.Queries.GetOperatingHours;

public class GetOperatingHoursQueryHandler : IQueryHandler<GetOperatingHoursQuery, GetOperatingHoursResponse>
{
    private readonly IVenueRepository _venueRepository;
    private readonly IApplicationDbContext _dbContext;

    public GetOperatingHoursQueryHandler(
        IVenueRepository venueRepository,
        IApplicationDbContext dbContext)
    {
        _venueRepository = venueRepository;
        _dbContext = dbContext;
    }

    public async Task<Result<GetOperatingHoursResponse>> Handle(
        GetOperatingHoursQuery request,
        CancellationToken cancellationToken)
    {
        // Check if venue exists
        var venue = await _venueRepository.GetByIdAsync(request.VenueId);
        if (venue is null)
        {
            return Result.Failure<GetOperatingHoursResponse>(VenueErrors.NotFound(request.VenueId));
        }

        // Get operating hours ordered by DayOfWeek
        var operatingHours = await _dbContext.VenueOperatingHours
            .AsNoTracking()
            .Where(oh => oh.VenueId == request.VenueId)
            .ToListAsync(cancellationToken);

        var response = new GetOperatingHoursResponse
        {
            VenueId = request.VenueId,
            OperatingHours = operatingHours
                .OrderBy(h => (int)h.DayOfWeek)
                .Select(h => new OperatingHourDto(
                    h.HoursId,
                    (int)h.DayOfWeek,
                    h.DayOfWeek.ToString(),
                    h.OpenTime,
                    h.CloseTime,
                    h.IsClosed
                )).ToList()
        };

        return Result.Success(response);
    }
}
