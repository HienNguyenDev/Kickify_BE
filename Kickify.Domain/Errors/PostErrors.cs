using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class PostErrors
{
    public static Error NotFound(Guid postId) => Error.NotFound(
        "Posts.NotFound",
        $"The post with the Id = '{postId}' was not found");

    public static readonly Error ContentOrMediaRequired = Error.Conflict(
        "Posts.ContentOrMediaRequired",
        "Post must have content or at least one media file");

    public static readonly Error MaxFilesExceeded = Error.Conflict(
        "Posts.MaxFilesExceeded",
        "Maximum 10 files allowed per post");

    public static Error UploadFailed(string errors) => Error.Conflict(
        "Posts.UploadFailed",
        $"Failed to upload media files: {errors}");

    public static readonly Error CreateFailed = Error.Conflict(
        "Posts.CreateFailed",
        "Failed to create post");

    public static readonly Error UpdateFailed = Error.Conflict(
        "Posts.UpdateFailed",
        "Failed to update post");

    public static readonly Error DeleteFailed = Error.Conflict(
        "Posts.DeleteFailed",
        "Failed to delete post");

    public static readonly Error Unauthorized = Error.Conflict(
        "Posts.Unauthorized",
        "You are not authorized to perform this action on this post");

    public static readonly Error InvalidVisibility = Error.Conflict(
        "Posts.InvalidVisibility",
        "Invalid post visibility setting");
}
