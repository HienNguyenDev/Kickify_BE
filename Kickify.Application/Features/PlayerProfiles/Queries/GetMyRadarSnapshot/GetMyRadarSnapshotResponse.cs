namespace Kickify.Application.Features.PlayerProfiles.Queries.GetMyRadarSnapshot;

public record GetMyRadarSnapshotResponse(
    decimal Form,
    decimal WinRate,
    decimal CommunityScore,
    decimal Trust,
    decimal Contribution,
    List<RadarAssessmentItem> Assessments,
    string Summary,
    DateTime UpdatedAt
);

public record RadarAssessmentItem(
    string Type,
    string Title,
    string Description,
    string Icon,
    string HighlightAxis
);
