namespace Kickify.Api.Requests;

public sealed record CreateHolidayRequest(
    DateTime Date,
    string Name
);
