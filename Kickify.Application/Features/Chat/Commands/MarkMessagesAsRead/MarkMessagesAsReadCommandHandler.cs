using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Chat.Commands.MarkMessagesAsRead;

public class MarkMessagesAsReadCommandHandler : ICommandHandler<MarkMessagesAsReadCommand, MarkMessagesAsReadCommandResponse>
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IChatHubService _chatHubService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public MarkMessagesAsReadCommandHandler(
        IChatMessageRepository chatMessageRepository,
        IChatHubService chatHubService,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _chatMessageRepository = chatMessageRepository;
        _chatHubService = chatHubService;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<MarkMessagesAsReadCommandResponse>> Handle(MarkMessagesAsReadCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = request.CurrentUserId ?? _userContext.UserId;

        await _chatMessageRepository.MarkAsReadAsync(currentUserId, request.FromUserId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _chatHubService.NotifyMessageReadAsync(request.FromUserId, currentUserId, cancellationToken);

        var response = new MarkMessagesAsReadCommandResponse
        {
            FromUserId = request.FromUserId,
            Success = true
        };

        return Result.Success(response);
    }
}
