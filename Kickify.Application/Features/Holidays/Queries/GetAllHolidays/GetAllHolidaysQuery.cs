using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Holidays.Queries.GetAllHolidays;

public record GetAllHolidaysQuery() : IQuery<GetAllHolidaysResponse>;