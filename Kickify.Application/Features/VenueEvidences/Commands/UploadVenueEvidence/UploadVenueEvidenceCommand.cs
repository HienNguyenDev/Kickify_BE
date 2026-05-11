using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Services;

namespace Kickify.Application.Features.VenueEvidences.Commands.UploadVenueEvidence;

public record UploadVenueEvidenceCommand(
    Guid VenueId,
    List<FileUploadRequest> Files
) : ICommand<UploadVenueEvidenceResponse>;
