using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Abstractions.Services
{
    public interface IStorageService
    {
        Task<UploadResult> UploadAsync(
            Stream stream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            string objectName,
            CancellationToken cancellationToken = default);

        Task<List<UploadResult>> UploadMultipleAsync(
            List<FileUploadRequest> files,
            CancellationToken cancellationToken = default);
    }

    public record UploadResult(
        bool Success,
        string ObjectName,
        string PublicUrl,
        long FileSize,
        string? ErrorMessage = null);

    public record FileUploadRequest(
        Stream Stream,
        string FileName,
        string ContentType,
        long FileSize);
}
