using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Holidays.Queries.GetAllHolidays;

public class GetAllHolidaysQueryHandler : IQueryHandler<GetAllHolidaysQuery, GetAllHolidaysResponse>
{
    private readonly IHolidayRepository _holidayRepository;

    public GetAllHolidaysQueryHandler(IHolidayRepository holidayRepository)
    {
        _holidayRepository = holidayRepository;
    }

    public async Task<Result<GetAllHolidaysResponse>> Handle(GetAllHolidaysQuery request, CancellationToken cancellationToken)
    {
        var holidays = await _holidayRepository.GetAllAsync(cancellationToken);

        var response = new GetAllHolidaysResponse(
            holidays.Select(h => new HolidayItemDto(
                h.Id,
                h.Name,
                h.Date
            )).ToList());

        return Result.Success(response);
    }
}