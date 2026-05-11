namespace Kickify.Api.Requests;

public record FieldPeakHourDto(
    string StartTime,
    string EndTime,
    decimal SurchargeAmount,
    bool IsPercentage,
    List<string> ApplicableDays
);
