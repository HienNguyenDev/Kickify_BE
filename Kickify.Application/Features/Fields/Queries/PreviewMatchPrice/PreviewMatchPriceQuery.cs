using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Fields.Queries.PreviewMatchPrice;

public sealed record PreviewMatchPriceQuery(
    Guid FieldId,
    DateTime MatchDate,
    TimeSpan StartTime,
    int DurationMinutes) : IQuery<PreviewMatchPriceResponse>;