using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.DTOs;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Chat.Commands.SendPrivateMessage;

public class SendPrivateMessageCommandHandler : ICommandHandler<SendPrivateMessageCommand, SendPrivateMessageCommandResponse>
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IChatHubService _chatHubService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public SendPrivateMessageCommandHandler(
        IChatMessageRepository chatMessageRepository,
        IUserRepository userRepository,
        IFriendshipRepository friendshipRepository,
        IChatHubService chatHubService,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _chatMessageRepository = chatMessageRepository;
        _userRepository = userRepository;
        _friendshipRepository = friendshipRepository;
        _chatHubService = chatHubService;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<SendPrivateMessageCommandResponse>> Handle(SendPrivateMessageCommand request, CancellationToken cancellationToken)
    {
        var senderId = request.SenderId ?? _userContext.UserId;

        if (request.ReceiverId == senderId)
        {
            return Result.Failure<SendPrivateMessageCommandResponse>(ChatErrors.CannotMessageSelf);
        }

        var receiver = await _userRepository.GetByIdAsync(request.ReceiverId);
        if (receiver is null)
        {
            return Result.Failure<SendPrivateMessageCommandResponse>(ChatErrors.ReceiverNotFound);
        }

        var areFriends = await _friendshipRepository.AreFriendsAsync(senderId, request.ReceiverId, cancellationToken);
        if (!areFriends)
        {
            return Result.Failure<SendPrivateMessageCommandResponse>(FriendshipErrors.CannotChat);
        }

        var sender = await _userRepository.GetByIdAsync(senderId);

        var message = new ChatMessage
        {
            MessageId = Guid.NewGuid(),
            SenderId = senderId,
            ReceiverId = request.ReceiverId,
            ConversationType = ConversationType.Private,
            MessageText = request.MessageText,
            MessageType = request.MessageType,
            SentAt = DateTime.UtcNow
        };

        await _chatMessageRepository.AddAsync(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var messageDto = new ChatMessageDto
        {
            MessageId = message.MessageId,
            SenderId = message.SenderId,
            SenderName = sender?.FullName ?? string.Empty,
            SenderAvatarUrl = sender?.AvatarUrl,
            ReceiverId = message.ReceiverId!.Value,
            MessageText = message.MessageText,
            MessageType = message.MessageType.ToString(),
            IsRead = false,
            SentAt = message.SentAt
        };

        await _chatHubService.SendMessageToUserAsync(request.ReceiverId, messageDto, cancellationToken);
        await _chatHubService.SendMessageToUserAsync(senderId, messageDto, cancellationToken);

        var response = new SendPrivateMessageCommandResponse
        {
            MessageId = message.MessageId,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId!.Value,
            MessageText = message.MessageText,
            MessageType = message.MessageType.ToString(),
            SentAt = message.SentAt
        };

        return Result.Success(response);
    }
}
