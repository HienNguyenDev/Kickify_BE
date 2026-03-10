using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Commands.SubmitVenueVerification;

public record SubmitVenueVerificationCommand(Guid VenueId) : ICommand<SubmitVenueVerificationResponse>;
