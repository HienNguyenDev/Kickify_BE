using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface IChatMessageRepository : IGenericRepository<ChatMessage>
{
    Task<(IEnumerable<ChatMessage> Messages, int Total)> GetPrivateConversationAsync(
        Guid userId1,
        Guid userId2,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(
        Guid receiverId,
        Guid senderId,
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(
        Guid receiverId,
        Guid senderId,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<(User OtherUser, ChatMessage LastMessage, int UnreadCount)>> GetConversationListAsync(
        Guid userId,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<int> GetConversationCountAsync(
        Guid userId,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
}
