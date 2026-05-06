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

    public static Error InvalidDateRange(DateTime startDate, DateTime endDate) => Error.Conflict(
        "Holidays.InvalidDateRange",
        $"The end date '{endDate:yyyy-MM-dd}' must be on or after the start date '{startDate:yyyy-MM-dd}'.");

    public static Error DatesAlreadyExist(string conflicts) => Error.Conflict(
        "Holidays.DatesAlreadyExist",
        $"The following dates are already configured as holidays: {conflicts}");

    public static Error InvalidIds(IEnumerable<Guid> holidayIds) => Error.NotFound(
        "Holidays.InvalidIds",
        $"Some holiday IDs were not found: {string.Join(", ", holidayIds)}");
}