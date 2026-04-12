using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.PlayerReports.Commands.ProcessReport;

public class ProcessReportCommandHandler : ICommandHandler<ProcessReportCommand, ProcessReportResponse>
{
    private readonly IPlayerReportRepository _reportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public ProcessReportCommandHandler(
        IPlayerReportRepository reportRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<ProcessReportResponse>> Handle(ProcessReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdWithDetailsAsync(request.ReportId, cancellationToken);
        if (report == null)
            return Result.Failure<ProcessReportResponse>(PlayerReportErrors.NotFound(request.ReportId));

        if (report.Status != ReportStatus.Pending)
            return Result.Failure<ProcessReportResponse>(PlayerReportErrors.AlreadyProcessed);

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        report.Status = request.IsApproved ? ReportStatus.Resolved : ReportStatus.Dismissed;
        report.AdminNotes = request.AdminNotes;
        report.ActionTaken = request.ActionTaken;
        report.ResolvedBy = _userContext.UserId;
        report.ResolvedAt = now;

        _reportRepository.Update(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProcessReportResponse(
            report.ReportId,
            report.Status.ToString(),
            report.AdminNotes,
            report.ActionTaken,
            now));
    }
}
