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
    }
}
