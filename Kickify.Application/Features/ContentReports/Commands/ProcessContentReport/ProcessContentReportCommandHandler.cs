using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.ContentReports.Commands.ProcessContentReport;

internal sealed class ProcessContentReportCommandHandler : ICommandHandler<ProcessContentReportCommand, ProcessContentReportResponse>
{
    private readonly IContentReportRepository _contentReportRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessContentReportCommandHandler(
        IContentReportRepository contentReportRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _contentReportRepository = contentReportRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProcessContentReportResponse>> Handle(ProcessContentReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _contentReportRepository.GetByIdWithDetailsAsync(request.ReportId, cancellationToken);
        if (report is null)
            return Result.Failure<ProcessContentReportResponse>(ContentReportErrors.NotFound(request.ReportId));

        if (report.Status != ReportStatus.Pending)
            return Result.Failure<ProcessContentReportResponse>(ContentReportErrors.AlreadyProcessed);

        report.Status = request.Action;
        report.AdminNotes = request.AdminNotes;
        report.ResolvedBy = _userContext.UserId;
        report.ResolvedAt = DateTime.UtcNow;

        _contentReportRepository.Update(report);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProcessContentReportResponse
        {
            ReportId = report.ReportId,
            Status = report.Status.ToString()
        });
    }
}
