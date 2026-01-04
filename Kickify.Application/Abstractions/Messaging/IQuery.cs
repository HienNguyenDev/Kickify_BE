using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;