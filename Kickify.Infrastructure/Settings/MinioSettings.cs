using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Infrastructure.Settings
{
    public class MinioSettings
    {
        public const string SectionName = "MinioSettings";

        public string Endpoint { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public bool UseSSL { get; set; } = false;
        public string PublicEndpoint { get; set; } = string.Empty;
    }
}
