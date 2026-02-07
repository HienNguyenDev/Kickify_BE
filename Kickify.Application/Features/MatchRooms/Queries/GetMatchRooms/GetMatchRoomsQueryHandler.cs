using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRooms
{
    public class GetMatchRoomsQueryHandler : IQueryHandler<GetMatchRoomsQuery, GetMatchRoomsResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IVenuePhotoRepository _venuePhotoRepository;

        public GetMatchRoomsQueryHandler(
            IMatchRoomRepository matchRoomRepository,
            IVenuePhotoRepository venuePhotoRepository)
        {
            _matchRoomRepository = matchRoomRepository;
            _venuePhotoRepository = venuePhotoRepository;
        }

        public async Task<Result<GetMatchRoomsResponse>> Handle(GetMatchRoomsQuery request, CancellationToken cancellationToken)
        {
            var (rooms, total) = await _matchRoomRepository.SearchRoomsAsync(
                request.Date,
                request.MatchFormat,
                request.AvailableOnly,
                request.Page,
                request.PageSize,
                cancellationToken
            );

            // Get unique venue IDs to fetch photos
            var venueIds = rooms
                .Where(r => r.Field?.Venue != null)
                .Select(r => r.Field!.Venue.VenueId)
                .Distinct()
                .ToList();

            // Fetch photos for all venues in a single query (avoid DbContext concurrency issues)
            var venuePhotosDict = new Dictionary<Guid, List<VenuePhotoDto>>();
            if (venueIds.Any())
            {
                var photosDict = await _venuePhotoRepository.GetPhotosByVenueIdsAsync(venueIds, cancellationToken);
                
                foreach (var kvp in photosDict)
                {
                    venuePhotosDict[kvp.Key] = kvp.Value
                        .Select(p => new VenuePhotoDto(p.PhotoId, p.PhotoUrl, p.DisplayOrder))
                        .ToList();
                }
            }

            var roomItems = rooms.Select(r =>
            {
                var endTime = r.StartTime.Add(TimeSpan.FromMinutes(r.DurationMinutes));

                // Get venue photos if venue exists
                var venuePhotos = new List<VenuePhotoDto>();
                if (r.Field?.Venue != null && venuePhotosDict.ContainsKey(r.Field.Venue.VenueId))
                {
                    venuePhotos = venuePhotosDict[r.Field.Venue.VenueId];
                }

                return new MatchRoomItemDto(
                    r.RoomId,
                    r.RoomName,
                    r.HostId,
                    r.Host.FullName ?? "Unknown",
                    r.Host.AvatarUrl,
                    r.FieldId,
                    r.Field?.FieldName,
                    r.Field?.Venue.VenueName,
                    r.Field?.Venue.Address,
                    venuePhotos,
                    r.MatchDate,
                    r.StartTime,
                    endTime,
                    r.DurationMinutes,
                    r.MatchFormat.ToString(),
                    r.TotalSlots,
                    r.FilledSlots,
                    r.DepositPerPerson,
                    r.Visibility.ToString(),
                    r.Visibility == Domain.Enums.Visibility.Private,
                    r.Status.ToString(),
                    r.CreatedAt
                );
            }).ToList();

            var response = new GetMatchRoomsResponse(
                roomItems,
                total,
                request.Page,
                request.PageSize,
                (int)Math.Ceiling(total / (double)request.PageSize)
            );

            return Result.Success(response);
        }
    }
}
