namespace Kickify.Application.Features.VenueEvidences.Queries.GetVenueEvidences;

public class GetVenueEvidencesResponse
{
    public List<EvidenceDto> Evidences { get; set; } = [];
}

public class EvidenceDto
{
    public Guid EvidenceId { get; set; }
    public Guid VenueId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}
