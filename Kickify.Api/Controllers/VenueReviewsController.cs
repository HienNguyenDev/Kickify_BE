using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.VenueReviews.Commands.DeleteVenueReview;
using Kickify.Application.Features.VenueReviews.Commands.ReplyVenueReview;
using Kickify.Application.Features.VenueReviews.Commands.UpdateReplyVenueReview;
using Kickify.Application.Features.VenueReviews.Queries.GetAllVenueReviews;
using Kickify.Application.Features.VenueReviews.Queries.GetVenueOwnerReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[ApiController]
[Route("api/venue-reviews")]
[Authorize]
public class VenueReviewsController : ControllerBase
{
    private readonly ISender _sender;

    public VenueReviewsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all venue reviews with optional filters and pagination
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetAllVenueReviews(
        [FromQuery] Guid? venueId,
        [FromQuery] Guid? userId,
        [FromQuery] int? minRating,
        [FromQuery] int? maxRating,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllVenueReviewsQuery(venueId, userId, minRating, maxRating, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Delete a venue review (Admin only, soft delete)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{reviewId:guid}")]
    public async Task<IResult> DeleteVenueReview(Guid reviewId, CancellationToken cancellationToken)
    {
        var command = new DeleteVenueReviewCommand(reviewId);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Venue owner replies to a player's review for one of their venues (one reply per review).
    /// </summary>
    [Authorize(Roles = "VenueOwner")]
    [HttpPost("{reviewId:guid}/reply")]
    public async Task<IResult> ReplyToVenueReview(
        Guid reviewId,
        [FromBody] ReplyVenueReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new ReplyVenueReviewCommand(reviewId, request.Message);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Venue owner updates an existing reply (same body as create reply).
    /// </summary>
    [Authorize(Roles = "VenueOwner")]
    [HttpPut("{reviewId:guid}/reply")]
    public async Task<IResult> UpdateVenueReviewReply(
        Guid reviewId,
        [FromBody] ReplyVenueReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateReplyVenueReviewCommand(reviewId, request.Message);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Get reviews for venues owned by the authenticated venue owner, with optional filters
    /// </summary>
    [Authorize(Roles = "VenueOwner")]
    [HttpGet("my-venues")]
    public async Task<IResult> GetVenueOwnerReviews(
        [FromQuery] Guid? venueId,
        [FromQuery] int? minRating,
        [FromQuery] int? maxRating,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVenueOwnerReviewsQuery(venueId, minRating, maxRating, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }
}
