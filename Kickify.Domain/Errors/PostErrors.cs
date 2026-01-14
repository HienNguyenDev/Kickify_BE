using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class PostErrors
{
    public static Error NotFound(Guid postId) => Error.NotFound(
        "Posts.NotFound",
        $"The post with the Id = '{postId}' was not found");

    public static readonly Error ContentOrMediaRequired = Error.Problem(
        "Posts.ContentOrMediaRequired",
        "Post must have content or at least one media file");

    public static readonly Error MaxFilesExceeded = Error.Problem(
        "Posts.MaxFilesExceeded",
        "Maximum 10 files allowed per post");

    public static Error UploadFailed(string errors) => Error.Problem(
        "Posts.UploadFailed",
        $"Failed to upload media files: {errors}");

    public static readonly Error CreateFailed = Error.Problem(
        "Posts.CreateFailed",
        "Failed to create post");

    public static readonly Error UpdateFailed = Error.Problem(
        "Posts.UpdateFailed",
        "Failed to update post");

    public static readonly Error DeleteFailed = Error.Problem(
        "Posts.DeleteFailed",
        "Failed to delete post");

    public static readonly Error Unauthorized = Error.Problem(
        "Posts.Unauthorized",
        "You are not authorized to perform this action on this post");

    public static readonly Error InvalidVisibility = Error.Problem(
        "Posts.InvalidVisibility",
        "Invalid post visibility setting");
}
