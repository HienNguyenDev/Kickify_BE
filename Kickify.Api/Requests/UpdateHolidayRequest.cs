namespace Kickify.Api.Requests;

public sealed record UpdateHolidayRequest(
    DateTime Date,
    string Name
);
