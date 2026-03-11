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

    public static readonly Error CannotMessageSelf = Error.Conflict(
        "Chat.CannotMessageSelf",
        "You cannot send a message to yourself");

    public static readonly Error MessageContentRequired = Error.Conflict(
        "Chat.MessageContentRequired",
        "Message content is required");

    public static readonly Error Unauthorized = Error.Conflict(
        "Chat.Unauthorized",
        "You are not authorized to perform this action");

    public static readonly Error NotRoomParticipant = Error.Conflict(
        "Chat.NotRoomParticipant",
        "You are not a participant of this room");

    public static readonly Error CannotSendToTeamChannel = Error.Conflict(
        "Chat.CannotSendToTeamChannel",
        "You can only send messages to your team channel");

    public static readonly Error CannotAccessTeamChannel = Error.Conflict(
        "Chat.CannotAccessTeamChannel",
        "You can only access your team channel");
}
