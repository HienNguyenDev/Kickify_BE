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
        var (items, totalCount) = await _holidayRepository.SearchHolidaysAsync(
            request.Keyword,
            request.Year,
            request.Page,
            request.PageSize,
            cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var response = new GetAllHolidaysResponse(
            items.Select(h => new HolidayItemDto(
                h.Id,
                h.Name,
                h.Date
            )).ToList(),
            totalCount,
            request.Page,
            request.PageSize,
            totalPages);

        return Result.Success(response);
    }
}