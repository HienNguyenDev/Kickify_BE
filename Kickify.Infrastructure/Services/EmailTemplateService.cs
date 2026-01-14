using Kickify.Application.Abstractions.Services;
using RazorLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Infrastructure.Services
{
    public class EmailTemplateService
    {
        private readonly RazorLightEngine _razorEngine;

        public EmailTemplateService()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var assemblyLocation = Path.GetDirectoryName(entryAssembly!.Location);
            var templatePath = Path.Combine(assemblyLocation!, "EmailTemplates");

            _razorEngine = new RazorLightEngineBuilder()
                .UseFileSystemProject(templatePath)
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task<string> RenderOtpEmailAsync(string otp)
        {
            return await _razorEngine.CompileRenderAsync("OtpEmail.cshtml", otp);
        }

        public async Task<string> RenderResetPasswordEmailAsync(string newPassword)
        {
            return await _razorEngine.CompileRenderAsync("ResetPasswordEmail.cshtml", newPassword);
        }
    }
}
