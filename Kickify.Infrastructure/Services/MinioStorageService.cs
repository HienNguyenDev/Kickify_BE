using Kickify.Application.Abstractions.Services;
using Kickify.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Infrastructure.Services
{
    public class MinioStorageService : IStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly MinioSettings _settings;

        private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"
        };

        private static readonly HashSet<string> AllowedVideoTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "video/mp4", "video/webm", "video/quicktime", "video/mpeg"
        };

        private static readonly HashSet<string> AllowedDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

        private const long MaxImageSize = 10 * 1024 * 1024;
        private const long MaxVideoSize = 100 * 1024 * 1024;
        private const long MaxDocumentSize = 20 * 1024 * 1024;

        public MinioStorageService(IMinioClient minioClient, IOptions<MinioSettings> settings)
        {
            _minioClient = minioClient;
            _settings = settings.Value;
        }

        public async Task<UploadResult> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
                var validationError = ValidateFile(fileName, contentType, stream.Length);
                if (validationError != null)
                {
                    return new UploadResult(false, "", "", 0, validationError);
                }

                // Ensure bucket exists
                await EnsureBucketExistsAsync(cancellationToken);

                // Generate unique object name
                var objectName = GenerateObjectName(fileName, contentType);

                // Upload
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

                var publicUrl = $"{_settings.PublicEndpoint}/{objectName}";

                return new UploadResult(true, objectName, publicUrl, stream.Length);
        }

        public async Task<List<UploadResult>> UploadMultipleAsync(List<FileUploadRequest> files, CancellationToken cancellationToken = default)
        {
            var results = new List<UploadResult>();

            var semaphore = new SemaphoreSlim(5);
            var tasks = files.Select(async file =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await UploadAsync(file.Stream, file.FileName, file.ContentType, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            results.AddRange(await Task.WhenAll(tasks));
            return results;
        }

        public async Task<bool> DeleteAsync(string objectName,CancellationToken cancellationToken = default)
        {
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(objectName);

                await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);

                return true;
        }

        private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(_settings.BucketName);

            bool exists = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);

            if (!exists)
            {
                var makeBucketArgs = new MakeBucketArgs()
                    .WithBucket(_settings.BucketName);

                await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);

                var policy = $$"""
                {
                    "Version": "2012-10-17",
                    "Statement": [
                        {
                            "Effect": "Allow",
                            "Principal": {"AWS": ["*"]},
                            "Action": ["s3:GetObject"],
                            "Resource": ["arn:aws:s3:::{{_settings.BucketName}}/*"]
                        }
                    ]
                }
                """;

                var setPolicyArgs = new SetPolicyArgs()
                    .WithBucket(_settings.BucketName)
                    .WithPolicy(policy);

                await _minioClient.SetPolicyAsync(setPolicyArgs, cancellationToken);
            }
        }

        private static string GenerateObjectName(string fileName, string contentType)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var folder = contentType.StartsWith("video/") ? "videos"
                : AllowedDocumentTypes.Contains(contentType) ? "documents"
                : "images";
            var datePath = DateTime.UtcNow.ToString("yyyy/MM/dd");
            var uniqueId = Guid.NewGuid().ToString("N")[..12];

            return $"{folder}/{datePath}/{uniqueId}{extension}";
        }

        private static string? ValidateFile(string fileName, string contentType, long fileSize)
        {
            // Check content type
            bool isImage = AllowedImageTypes.Contains(contentType);
            bool isVideo = AllowedVideoTypes.Contains(contentType);
            bool isDocument = AllowedDocumentTypes.Contains(contentType);

            if (!isImage && !isVideo && !isDocument)
            {
                return $"File type '{contentType}' is not allowed";
            }

            // Check size
            if (isImage && fileSize > MaxImageSize)
                return $"Image size exceeds limit of {MaxImageSize / 1024 / 1024}MB";

            if (isVideo && fileSize > MaxVideoSize)
                return $"Video size exceeds limit of {MaxVideoSize / 1024 / 1024}MB";

            if (isDocument && fileSize > MaxDocumentSize)
                return $"Document size exceeds limit of {MaxDocumentSize / 1024 / 1024}MB";

            // Check extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4", ".webm", ".mov", ".pdf", ".doc", ".docx" };

            if (!allowedExtensions.Contains(extension))
            {
                return $"File extension '{extension}' is not allowed";
            }

            return null;
        }
    }
}
