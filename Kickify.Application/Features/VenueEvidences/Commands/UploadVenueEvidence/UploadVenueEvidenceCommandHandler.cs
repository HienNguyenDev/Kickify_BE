using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.VenueEvidences.Commands.UploadVenueEvidence;

internal sealed class UploadVenueEvidenceCommandHandler : ICommandHandler<UploadVenueEvidenceCommand, UploadVenueEvidenceResponse>
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp",
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/msword"
    ];

    private readonly IVenueEvidenceRepository _evidenceRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IStorageService _storageService;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public UploadVenueEvidenceCommandHandler(
        IVenueEvidenceRepository evidenceRepository,
        IVenueRepository venueRepository,
        IStorageService storageService,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _evidenceRepository = evidenceRepository;
        _venueRepository = venueRepository;
        _storageService = storageService;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UploadVenueEvidenceResponse>> Handle(UploadVenueEvidenceCommand request, CancellationToken cancellationToken)
    {
        var venue = await _venueRepository.GetVenueForUpdateAsync(request.VenueId, cancellationToken);
        if (venue is null)
            return Result.Failure<UploadVenueEvidenceResponse>(VenueErrors.NotFound(request.VenueId));

        if (venue.OwnerId != _userContext.UserId)
            return Result.Failure<UploadVenueEvidenceResponse>(VenueErrors.Unauthorized);

        var invalidFile = request.Files.FirstOrDefault(f => !AllowedContentTypes.Contains(f.ContentType.ToLower()));
        if (invalidFile is not null)
            return Result.Failure<UploadVenueEvidenceResponse>(VenueErrors.InvalidEvidenceFileType);

        var uploadResults = await _storageService.UploadMultipleAsync(request.Files, cancellationToken);

        var evidences = new List<VenueEvidence>();
        var dtos = new List<EvidenceUploadedDto>();

        for (var i = 0; i < uploadResults.Count; i++)
        {
            var upload = uploadResults[i];
            if (!upload.Success) continue;

            var evidence = new VenueEvidence
            {
                EvidenceId = Guid.NewGuid(),
                VenueId = request.VenueId,
                FileUrl = upload.PublicUrl,
                FileName = request.Files[i].FileName,
                ContentType = request.Files[i].ContentType,
                FileSize = upload.FileSize,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            evidences.Add(evidence);
            dtos.Add(new EvidenceUploadedDto
            {
                EvidenceId = evidence.EvidenceId,
                FileName = evidence.FileName,
                FileUrl = evidence.FileUrl,
                ContentType = evidence.ContentType,
                FileSize = evidence.FileSize,
                CreatedAt = evidence.CreatedAt
            });
        }

        foreach (var evidence in evidences)
            await _evidenceRepository.AddAsync(evidence);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UploadVenueEvidenceResponse { Evidences = dtos });
    }
}
