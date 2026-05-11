using Kickify.Domain.Common;
using Kickify.Domain.Enums;

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

        public static readonly Error InvalidLocation = Error.Conflict(
            "Venues.InvalidLocation",
            "Invalid latitude or longitude coordinates");

        public static readonly Error InvalidOperatingHours = Error.Conflict(
            "Venues.InvalidOperatingHours",
            "Invalid operating hours. Check day of week and time values");

        public static readonly Error NoFieldsProvided = Error.Conflict(
            "Venues.NoFieldsProvided",
            "At least one field must be provided when creating a venue");

        public static readonly Error Unauthorized = Error.Conflict(
            "Venues.Unauthorized",
            "You are not authorized to perform this action on this venue");

        public static Error InvalidFieldType(string fieldType) => Error.Conflict(
            "Venues.InvalidFieldType",
            $"Invalid field type: {fieldType}");

        public static readonly Error CreateFailed = Error.Conflict(
            "Venues.CreateFailed",
            "Failed to create venue");

        public static Error UploadFailed(string errors) => Error.Conflict(
            "Venues.UploadFailed",
            $"Failed to upload venue photos: {errors}");

        public static readonly Error NoPhotosProvided = Error.Conflict(
            "Venues.NoPhotosProvided",
            "At least one photo must be provided");

        public static Error InvalidStatus(string status) => Error.Conflict(
            "Venues.InvalidStatus",
            $"Invalid venue status: '{status}'. Allowed values: Draft, PendingVerification, Approved, Rejected, Suspended, Archived");

        public static Error CannotToggleSuspension(string currentStatus) => Error.Conflict(
            "Venues.CannotToggleSuspension",
            $"Cannot toggle suspension when venue status is '{currentStatus}'. Only Approved or Suspended venues can be toggled");
        public static Error CannotToggleArchived(string currentStatus) => Error.Problem(
            "Venues.CannotToggleArchived",
            $"Cannot toggle archived when venue status is '{currentStatus}'. Only Approved or Archived venues can be toggled");

        public static readonly Error InsufficientPhotos = Error.Conflict(
            "Venues.InsufficientPhotos",
            "At least 1 venue photo is required before submitting for verification.");

        public static readonly Error InsufficientEvidences = Error.Conflict(
            "Venues.InsufficientEvidences",
            "At least 1 evidence document is required before submitting for verification.");

        public static readonly Error InvalidVerificationStatus = Error.Conflict(
            "Venues.InvalidVerificationStatus",
            "Only venues with status 'Draft' or 'Rejected' can be submitted for verification.");

        public static readonly Error EvidenceNotFound = Error.NotFound(
            "Venues.EvidenceNotFound",
            "The evidence document was not found.");

        public static readonly Error InvalidEvidenceFileType = Error.Conflict(
            "Venues.InvalidEvidenceFileType",
            "Invalid file type. Allowed types: images (jpg, png, etc.), PDF, DOCX.");

        public static Error InvalidPeakHourTimeFormat(string fieldName, int peakHourIndex, string? startTime, string? endTime) => Error.Conflict(
            "Venues.InvalidPeakHourTimeFormat",
            $"Field '{fieldName}' peak hour #{peakHourIndex}: invalid time format. StartTime='{startTime}', EndTime='{endTime}'. Expected format HH:mm:ss.");

        public static Error InvalidPeakHourTimeRange(string fieldName, int peakHourIndex, TimeSpan startTime, TimeSpan endTime) => Error.Conflict(
            "Venues.InvalidPeakHourTimeRange",
            $"Field '{fieldName}' peak hour #{peakHourIndex}: StartTime '{startTime}' must be earlier than EndTime '{endTime}'.");

        public static Error InvalidPeakHourApplicableDay(string fieldName, int peakHourIndex, string day) => Error.Conflict(
            "Venues.InvalidPeakHourApplicableDay",
            $"Field '{fieldName}' peak hour #{peakHourIndex}: invalid ApplicableDay '{day}'. Allowed values: Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday.");

        public static Error PeakHourApplicableDaysRequired(string fieldName, int peakHourIndex) => Error.Conflict(
            "Venues.PeakHourApplicableDaysRequired",
            $"Field '{fieldName}' peak hour #{peakHourIndex}: ApplicableDays is required and cannot be empty.");

        public static Error PeakHourDayOutsideVenueOpenDays(string fieldName, int peakHourIndex, DayOfWeekEnum day) => Error.Conflict(
            "Venues.PeakHourDayOutsideVenueOpenDays",
            $"Field '{fieldName}' peak hour #{peakHourIndex}: day '{day}' is not in venue open days.");
    }
}
