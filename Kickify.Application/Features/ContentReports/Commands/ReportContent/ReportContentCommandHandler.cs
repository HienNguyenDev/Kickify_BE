using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.ContentReports.Commands.ReportContent;

internal sealed class ReportContentCommandHandler : ICommandHandler<ReportContentCommand, ReportContentResponse>
{
    private readonly IContentReportRepository _contentReportRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public ReportContentCommandHandler(
        IContentReportRepository contentReportRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _contentReportRepository = contentReportRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReportContentResponse>> Handle(ReportContentCommand request, CancellationToken cancellationToken)
    {
        var reporterId = _userContext.UserId;

        Guid contentOwnerId;

        if (request.ContentType == ContentType.Post)
        {
            var post = await _postRepository.GetByIdAsync(request.ContentId);
            if (post is null)
                return Result.Failure<ReportContentResponse>(ContentReportErrors.ContentNotFound(ContentType.Post, request.ContentId));

            contentOwnerId = post.UserId;
        }
        else
        {
            var comment = await _commentRepository.GetByIdAsync(request.ContentId);
            if (comment is null)
                return Result.Failure<ReportContentResponse>(ContentReportErrors.ContentNotFound(ContentType.Comment, request.ContentId));

            contentOwnerId = comment.UserId;
        }

        if (contentOwnerId == reporterId)
            return Result.Failure<ReportContentResponse>(ContentReportErrors.SelfReport);

        var alreadyReported = await _contentReportRepository.HasAlreadyReportedAsync(reporterId, request.ContentId, cancellationToken);
        if (alreadyReported)
            return Result.Failure<ReportContentResponse>(ContentReportErrors.AlreadyReported);

        var report = new ContentReport
        {
            ReportId = Guid.NewGuid(),
            ReporterId = reporterId,
            ContentType = request.ContentType,
            ContentId = request.ContentId,
            ContentOwnerId = contentOwnerId,
            Reason = request.Reason,
            Description = request.Description,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _contentReportRepository.AddAsync(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ReportContentResponse
        {
            ReportId = report.ReportId,
            Message = "Your report has been submitted successfully."
        });
    }
}
