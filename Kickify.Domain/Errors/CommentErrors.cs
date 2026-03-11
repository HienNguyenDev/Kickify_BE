using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class CommentErrors
{
    public static Error NotFound(Guid commentId) => Error.NotFound("Comments.NotFound", $"The comment with the Id = '{commentId}' was not found");
    public static readonly Error PostNotFound = Error.NotFound("Comments.PostNotFound", "The post was not found");
    public static readonly Error Unauthorized = Error.Conflict("Comments.Unauthorized", "You are not authorized to perform this action on this comment");
    public static readonly Error ContentRequired = Error.Conflict("Comments.ContentRequired", "Comment content is required");
    public static readonly Error ParentCommentNotFound = Error.NotFound("Comments.ParentCommentNotFound", "The parent comment was not found");
    public static readonly Error AlreadyLiked = Error.Conflict("Comments.AlreadyLiked", "You have already liked this comment");
    public static readonly Error NotLiked = Error.Conflict("Comments.NotLiked", "You have not liked this comment");
}
