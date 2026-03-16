using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class HolidayErrors
{
    public static Error NotFound(Guid holidayId) => Error.NotFound(
        "Holidays.NotFound",
        $"The holiday with Id = '{holidayId}' was not found");

    public static Error AlreadyExistsOnDate(DateTime date) => Error.Conflict(
        "Holidays.AlreadyExistsOnDate",
        $"A holiday already exists on date '{date:yyyy-MM-dd}'");

    public static Error InvalidIds(IEnumerable<Guid> holidayIds) => Error.NotFound(
        "Holidays.InvalidIds",
        $"Some holiday IDs were not found: {string.Join(", ", holidayIds)}");
}