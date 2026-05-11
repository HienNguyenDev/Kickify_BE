using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.MatchFeedbacks.Commands.RespondToFeedback;

public class RespondToFeedbackCommandHandler : ICommandHandler<RespondToFeedbackCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public RespondToFeedbackCommandHandler(IApplicationDbContext dbContext, IUserContext userContext, IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Respond to a feedback that was received by current user.
    /// </summary>
    public async Task<Result> Handle(RespondToFeedbackCommand request, CancellationToken cancellationToken)
    {
        var feedback = await _dbContext.MatchFeedbacks
            .FirstOrDefaultAsync(x => x.FeedbackId == request.FeedbackId, cancellationToken);

        if (feedback is null)
        {
            return Result.Failure(MatchFeedbackErrors.NotFound(request.FeedbackId));
        }

        if (feedback.RevieweeId != _userContext.UserId)
        {
            return Result.Failure(MatchFeedbackErrors.Forbidden);
        }

        feedback.RevieweeResponse = request.Response.Trim();
        feedback.ResponseDate = DateTime.UtcNow;
        _dbContext.MatchFeedbacks.Update(feedback);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
