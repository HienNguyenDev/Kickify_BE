using FirebaseAdmin.Auth;
using Kickify.Application.Abstractions.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Infrastructure.Authentication
{
    public sealed class AuthenticationServices : IAuthenticationServices
    {
        public async Task<string> RegisterAsync(string email, string password)
        {
            var userArgs = new UserRecordArgs
            {
                Email = email,
                Password = password,
            };
            var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);

            return userRecord.Uid;
        }

        public async Task DeleteUserAsync(string identityId)
        {
            await FirebaseAuth.DefaultInstance.DeleteUserAsync(identityId);
        }
    }
}
