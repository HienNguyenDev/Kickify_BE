namespace Kickify.Application.Abstractions.Services;

public interface IQrCodeService
{
    /// <summary>
    /// Generates a QR code PNG image as a byte array for the given content.
    /// </summary>
    /// <param name="content">The content to encode in the QR code (e.g., a deep link URL)</param>
    /// <param name="pixelsPerModule">Size of each QR module in pixels (default: 10)</param>
    /// <returns>PNG image byte array</returns>
    byte[] GenerateQrCodePng(string content, int pixelsPerModule = 10);
}
