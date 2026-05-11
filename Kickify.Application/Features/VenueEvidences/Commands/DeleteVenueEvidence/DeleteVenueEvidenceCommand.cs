using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenueEvidences.Commands.DeleteVenueEvidence;

public record DeleteVenueEvidenceCommand(Guid EvidenceId) : ICommand<DeleteVenueEvidenceResponse>;
