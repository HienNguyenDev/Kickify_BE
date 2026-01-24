using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class ChatMessageRepository : GenericRepository<ChatMessage>, IChatMessageRepository
{
    public ChatMessageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<ChatMessage> Messages, int Total)> GetPrivateConversationAsync(
        Guid userId1,
        Guid userId2,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.ConversationType == ConversationType.Private &&
                ((m.SenderId == userId1 && m.ReceiverId == userId2) ||
                 (m.SenderId == userId2 && m.ReceiverId == userId1)));

        var total = await query.CountAsync(cancellationToken);

        var messages = await query
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (messages.OrderBy(m => m.SentAt), total);
    }

    public async Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(
        Guid receiverId,
        Guid senderId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.ReceiverId == receiverId &&
                        m.SenderId == senderId &&
                        m.ConversationType == ConversationType.Private &&
                        !m.IsRead)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(
        Guid receiverId,
        Guid senderId,
        CancellationToken cancellationToken = default)
    {
        await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE social.""ChatMessages"" 
              SET ""IsRead"" = true 
              WHERE ""ReceiverId"" = {0} AND ""SenderId"" = {1} AND ""IsRead"" = false",
            receiverId, senderId);
    }

    public async Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.ReceiverId == userId &&
                        m.ConversationType == ConversationType.Private &&
                        !m.IsRead)
            .CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<(User OtherUser, ChatMessage LastMessage, int UnreadCount)>> GetConversationListAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var conversationData = await _dbSet
            .Where(m => m.ConversationType == ConversationType.Private &&
                       (m.SenderId == userId || m.ReceiverId == userId))
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => new
            {
                OtherUserId = g.Key,
                LastMessageId = g.OrderByDescending(m => m.SentAt).Select(m => m.MessageId).FirstOrDefault(),
                UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
            })
            .ToListAsync(cancellationToken);

        if (!conversationData.Any())
            return Enumerable.Empty<(User, ChatMessage, int)>();

        var lastMessageIds = conversationData.Select(c => c.LastMessageId).ToList();
        var lastMessages = await _dbSet
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => lastMessageIds.Contains(m.MessageId))
            .ToListAsync(cancellationToken);

        var result = conversationData
            .Select(conv =>
            {
                var lastMessage = lastMessages.FirstOrDefault(m => m.MessageId == conv.LastMessageId);
                if (lastMessage == null) return ((User?, ChatMessage?, int))(null, null, 0);

                var otherUser = lastMessage.SenderId == userId ? lastMessage.Receiver : lastMessage.Sender;
                return (otherUser, lastMessage, conv.UnreadCount);
            })
            .Where(x => x.Item1 != null && x.Item2 != null)
            .OrderByDescending(x => x.Item2!.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => (x.Item1!, x.Item2!, x.Item3))
            .ToList();

        return result;
    }

    public async Task<int> GetConversationCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.ConversationType == ConversationType.Private &&
                       (m.SenderId == userId || m.ReceiverId == userId))
            .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Distinct()
            .CountAsync(cancellationToken);
    }
}
