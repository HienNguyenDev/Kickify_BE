namespace Kickify.Application.Features.VenueEvidences.Commands.UploadVenueEvidence;

public class UploadVenueEvidenceResponse
{
    public List<EvidenceUploadedDto> Evidences { get; set; } = [];
}

public class EvidenceUploadedDto
{
    public Guid EvidenceId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}
