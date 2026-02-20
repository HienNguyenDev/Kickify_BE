using Kickify.Application.Abstractions.Services;
using QRCoder;

namespace Kickify.Infrastructure.Services;

public class QrCodeService : IQrCodeService
{
    public byte[] GenerateQrCodePng(string content, int pixelsPerModule = 10)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        return qrCode.GetGraphic(pixelsPerModule);
    }
}
