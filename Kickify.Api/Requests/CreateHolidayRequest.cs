namespace Kickify.Api.Requests;

public sealed record CreateHolidayRequest(
    DateTime StartDate,
    DateTime EndDate,
    string Name
);
