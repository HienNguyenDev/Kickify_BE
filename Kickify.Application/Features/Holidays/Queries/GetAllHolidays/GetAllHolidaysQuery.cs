using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Holidays.Queries.GetAllHolidays;

public record GetAllHolidaysQuery(
	string? Keyword = null,
	int? Year = null,
	int Page = 1,
	int PageSize = 20
) : IQuery<GetAllHolidaysResponse>;