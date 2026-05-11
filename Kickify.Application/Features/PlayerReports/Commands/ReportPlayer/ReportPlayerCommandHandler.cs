using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.PlayerReports.Commands.ReportPlayer;

public class ReportPlayerCommandHandler : ICommandHandler<ReportPlayerCommand, ReportPlayerResponse>
{
    private readonly IPlayerReportRepository _reportRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public ReportPlayerCommandHandler(
        IPlayerReportRepository reportRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _reportRepository = reportRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<ReportPlayerResponse>> Handle(ReportPlayerCommand request, CancellationToken cancellationToken)
    {
        var reporterId = _userContext.UserId;

        if (reporterId == request.ReportedUserId)
            return Result.Failure<ReportPlayerResponse>(PlayerReportErrors.SelfReport);

        var reportedUser = await _userRepository.GetByIdAsync(request.ReportedUserId);
        if (reportedUser == null)
            return Result.Failure<ReportPlayerResponse>(UserErrors.NotFound(request.ReportedUserId));

        var hasPending = await _reportRepository.HasPendingReportAsync(reporterId, request.ReportedUserId, cancellationToken);
        if (hasPending)
            return Result.Failure<ReportPlayerResponse>(PlayerReportErrors.AlreadyReported);

        var report = new PlayerReport
        {
            ReportId = Guid.NewGuid(),
            ReporterId = reporterId,
            ReportedId = request.ReportedUserId,
            MatchId = request.MatchId,
            ReportType = request.ReportType,
            Description = request.Description,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _reportRepository.AddAsync(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ReportPlayerResponse(
            report.ReportId,
            report.ReportedId,
            report.ReportType.ToString(),
            report.Description,
            report.Status.ToString(),
            report.CreatedAt));
    }
}
