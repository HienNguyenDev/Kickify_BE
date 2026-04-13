using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.SystemLogs.Queries.GetSystemLogById;

public class GetSystemLogByIdQueryHandler
    : IQueryHandler<GetSystemLogByIdQuery, GetSystemLogByIdResponse>
{
    private readonly IApplicationDbContext _db;

    public GetSystemLogByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<GetSystemLogByIdResponse>> Handle(
        GetSystemLogByIdQuery request, CancellationToken cancellationToken)
    {
        var row = await _db.SystemLogs
            .AsNoTracking()
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.LogId == request.LogId, cancellationToken);

        if (row is null)
            return Result.Failure<GetSystemLogByIdResponse>(SystemLogErrors.NotFound(request.LogId));

        var email = row.User?.Email;

        var response = new GetSystemLogByIdResponse(
            row.LogId,
            row.UserId,
            row.UserName,
            email,
            row.Action.ToString(),
            row.EntityType,
            row.EntityId,
            row.UserAgent,
            row.ResponseStatus.ToString(),
            row.ErrorMessage,
            row.CreatedAt);

        return Result.Success(response);
    }
}
