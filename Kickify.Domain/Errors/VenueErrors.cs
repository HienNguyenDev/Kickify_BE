using Kickify.Domain.Common;

namespace Kickify.Domain.Errors
{
    public static class VenueErrors
    {
        public static Error NotFound(Guid? venueId) => Error.NotFound(
            "Venues.NotFound",
            $"The venue with Id = '{venueId}' was not found");

        public static readonly Error OwnerNotFound = Error.NotFound(
            "Venues.OwnerNotFound",
            "The venue owner was not found");

        public static readonly Error InvalidLocation = Error.Problem(
            "Venues.InvalidLocation",
            "Invalid latitude or longitude coordinates");

        public static readonly Error InvalidOperatingHours = Error.Problem(
            "Venues.InvalidOperatingHours",
            "Invalid operating hours. Check day of week and time values");

        public static readonly Error NoFieldsProvided = Error.Problem(
            "Venues.NoFieldsProvided",
            "At least one field must be provided when creating a venue");

        public static readonly Error Unauthorized = Error.Problem(
            "Venues.Unauthorized",
            "You are not authorized to perform this action on this venue");

        public static Error InvalidFieldType(string fieldType) => Error.Problem(
            "Venues.InvalidFieldType",
            $"Invalid field type: {fieldType}");

        public static readonly Error CreateFailed = Error.Problem(
            "Venues.CreateFailed",
            "Failed to create venue");

        public static Error UploadFailed(string errors) => Error.Problem(
            "Venues.UploadFailed",
            $"Failed to upload venue photos: {errors}");

        public static readonly Error NoPhotosProvided = Error.Problem(
            "Venues.NoPhotosProvided",
            "At least one photo must be provided");

        public static Error InvalidStatus(string status) => Error.Problem(
            "Venues.InvalidStatus",
            $"Invalid venue status: '{status}'. Allowed values: Pending, Active, Approved, Rejected, Suspended");
    }
}
