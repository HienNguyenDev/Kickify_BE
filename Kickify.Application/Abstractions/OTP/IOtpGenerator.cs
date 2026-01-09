using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Abstractions.OTP
{
    public interface IOtpGenerator
    {
        string Generate6Digits();
    }
}
