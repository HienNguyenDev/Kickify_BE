using Microsoft.AspNetCore.Http;

namespace Kickify.Api.Requests
{
    public class CheckInMatchRoomRequest
    {
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public IFormFile? Photo { get; set; }
    }
}
