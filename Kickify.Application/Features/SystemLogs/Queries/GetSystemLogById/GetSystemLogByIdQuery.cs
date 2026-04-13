using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.SystemLogs.Queries.GetSystemLogById;

public record GetSystemLogByIdQuery(Guid LogId) : IQuery<GetSystemLogByIdResponse>;
