using Kickify.Application.Abstractions.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Infrastructure.Authentication
{
    public class ResetPasswordGenerator : IResetPasswordGenerator
    {
        public string GenerateRandomPassword(int length = 10)
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";

            string allChars = upper + lower + digits;
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int idx = RandomNumberGenerator.GetInt32(allChars.Length);
                result.Append(allChars[idx]);
            }

            result[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
            result[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
            result[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];

            return result.ToString();
        }
    }
}
