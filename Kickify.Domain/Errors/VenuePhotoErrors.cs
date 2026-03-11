using Kickify.Domain.Common;

namespace Kickify.Domain.Errors
{
    public static class VenuePhotoErrors
    {
        public static Error NotFound(Guid photoId) => Error.NotFound(
            "VenuePhotos.NotFound",
            $"The venue photo with Id = '{photoId}' was not found");

        public static readonly Error VenueNotFound = Error.NotFound(
            "VenuePhotos.VenueNotFound",
            "The venue was not found");

        public static readonly Error Unauthorized = Error.Conflict(
            "VenuePhotos.Unauthorized",
            "You are not authorized to perform this action on this venue's photos");

        public static readonly Error NoPhotosProvided = Error.Conflict(
            "VenuePhotos.NoPhotosProvided",
            "At least one photo must be provided");

        public static Error UploadFailed(string errors) => Error.Conflict(
            "VenuePhotos.UploadFailed",
            $"Failed to upload venue photos: {errors}");

        public static readonly Error DeleteFailed = Error.Conflict(
            "VenuePhotos.DeleteFailed",
            "Failed to delete venue photo");

        public static readonly Error UpdateFailed = Error.Conflict(
            "VenuePhotos.UpdateFailed",
            "Failed to update venue photo");
    }
}
