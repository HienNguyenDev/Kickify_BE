using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class ChatErrors
{
    public static Error MessageNotFound(Guid messageId) => Error.NotFound(
        "Chat.MessageNotFound",
        $"The message with the Id = '{messageId}' was not found");

    public static readonly Error ReceiverNotFound = Error.NotFound(
        "Chat.ReceiverNotFound",
        "The receiver was not found");

    public static readonly Error CannotMessageSelf = Error.Problem(
        "Chat.CannotMessageSelf",
        "You cannot send a message to yourself");

    public static readonly Error MessageContentRequired = Error.Problem(
        "Chat.MessageContentRequired",
        "Message content is required");

    public static readonly Error Unauthorized = Error.Problem(
        "Chat.Unauthorized",
        "You are not authorized to perform this action");
}
