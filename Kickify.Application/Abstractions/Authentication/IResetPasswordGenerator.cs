using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Abstractions.Authentication
{
    public interface IResetPasswordGenerator
    {
        string GenerateRandomPassword(int length = 10);
    }
}
