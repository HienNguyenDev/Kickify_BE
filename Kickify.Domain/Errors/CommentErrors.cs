using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class CommentErrors
{
    public static Error NotFound(Guid commentId) => Error.NotFound("Comments.NotFound", $"The comment with the Id = '{commentId}' was not found");
    public static readonly Error PostNotFound = Error.NotFound("Comments.PostNotFound", "The post was not found");
    public static readonly Error Unauthorized = Error.Problem("Comments.Unauthorized", "You are not authorized to perform this action on this comment");
    public static readonly Error ContentRequired = Error.Problem("Comments.ContentRequired", "Comment content is required");
    public static readonly Error ParentCommentNotFound = Error.NotFound("Comments.ParentCommentNotFound", "The parent comment was not found");
    public static readonly Error AlreadyLiked = Error.Problem("Comments.AlreadyLiked", "You have already liked this comment");
    public static readonly Error NotLiked = Error.Problem("Comments.NotLiked", "You have not liked this comment");
}
