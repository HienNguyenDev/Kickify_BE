using Kickify.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Domain.Errors
{
    public static class UserErrors
    {
        public static Error NotFound(Guid? userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");

        public static readonly Error NotFoundByEmail = Error.NotFound(
            "Users.NotFoundByEmail",
            "The user with the specified email was not found");

        public static readonly Error EmailAlreadyExists = Error.Conflict(
           "Users.EmailAlreadyExists",
           "The provided email already exists.");

        public static readonly Error WrongPassword = Error.Conflict(
            "Users.WrongPassword",
            "The passsword for this account is wrong");

        public static readonly Error IsNotVerified = Error.Conflict(
            "NotVerified",
            "Account is not verified");

        public static readonly Error InActive = Error.Conflict(
            "Users.InActive",
            "The user is inactive");

        public static readonly Error InvalidEmail = Error.Problem(
            "Users.InvalidEmail",
            "The email format is invalid");

        public static readonly Error InvalidPassword = Error.Problem(
            "Users.InvalidPassword",
            "The password does not meet the requirements");

        public static readonly Error InvalidPhoneNumber = Error.Problem(
            "Users.InvalidPhoneNumber",
            "The phone number format is invalid");

        public static readonly Error InvalidDateOfBirth = Error.Problem(
            "Users.InvalidDateOfBirth",
            "The date of birth is invalid");

        public static readonly Error UserAlreadyDeleted = Error.Conflict(
            "Users.AlreadyDeleted",
            "The user has already been deleted");

        public static readonly Error CannotDeleteActiveUser = Error.Conflict(
            "Users.CannotDeleteActive",
            "Cannot delete an active user. Deactivate the user first.");

        public static readonly Error UpdateFailed = Error.Problem(
            "Users.UpdateFailed",
            "Failed to update user information");

        public static readonly Error CreateFailed = Error.Problem(
            "Users.CreateFailed",
            "Failed to create user");

        public static readonly Error DeleteFailed = Error.Problem(
            "Users.DeleteFailed",
            "Failed to delete user");
        public static readonly Error InvalidRefreshToken = Error.Problem(
            "User.InvalidRefreshToken",
            "Invalid refresh token");

        public static readonly Error RefreshTokenExpired = Error.Problem(
            "User.RefreshTokenExpired",
            "Refresh token has expired");

        public static readonly Error TokenReuseDetected = Error.Problem(
            "User.TokenReuseDetected",
            "Token reuse detected. All sessions have been revoked for security reasons.");
        public static readonly Error OtpExpired = Error.Conflict(
            "Users.OtpExpired",
            "The OTP code not found or expired");
        public static readonly Error WrongOtp = Error.Conflict(
            "Users.OtpNotFound",
            "The OTP code is wrong");
        public static readonly Error UserAlreadyVerified = Error.Conflict(
            "Users.UserAlreadyVerified",
            "The user has already verified");
    }
}
