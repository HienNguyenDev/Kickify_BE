using Kickify.Application.Abstractions.OTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Infrastructure.Authentication
{
    public class OtpGenerator : IOtpGenerator
    {
        private static readonly Random _rand = new();

        public string Generate6Digits() => _rand.Next(0, 1_000_000).ToString("D6");
    }
}
