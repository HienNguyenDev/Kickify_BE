using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.VenueEvidences.Queries.GetVenueEvidences;

internal sealed class GetVenueEvidencesQueryHandler : IQueryHandler<GetVenueEvidencesQuery, GetVenueEvidencesResponse>
{
    private readonly IVenueEvidenceRepository _evidenceRepository;
    private readonly IVenueRepository _venueRepository;

    public GetVenueEvidencesQueryHandler(
        IVenueEvidenceRepository evidenceRepository,
        IVenueRepository venueRepository)
    {
        _evidenceRepository = evidenceRepository;
        _venueRepository = venueRepository;
    }

    public async Task<Result<GetVenueEvidencesResponse>> Handle(GetVenueEvidencesQuery request, CancellationToken cancellationToken)
    {
        var venueExists = await _venueRepository.GetVenueForUpdateAsync(request.VenueId, cancellationToken);
        if (venueExists is null)
            return Result.Failure<GetVenueEvidencesResponse>(VenueErrors.NotFound(request.VenueId));

        var evidences = await _evidenceRepository.GetByVenueIdAsync(request.VenueId, cancellationToken);

        var dtos = evidences.Select(e => new EvidenceDto
        {
            EvidenceId = e.EvidenceId,
            VenueId = e.VenueId,
            FileName = e.FileName,
            FileUrl = e.FileUrl,
            ContentType = e.ContentType,
            FileSize = e.FileSize,
            CreatedAt = e.CreatedAt
        }).ToList();

        return Result.Success(new GetVenueEvidencesResponse { Evidences = dtos });
    }
}
