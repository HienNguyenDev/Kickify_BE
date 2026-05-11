using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.MatchRooms.Queries.GetRecommendedMatchRooms
{
    public class GetRecommendedMatchRoomsQueryHandler : IQueryHandler<GetRecommendedMatchRoomsQuery, GetRecommendedMatchRoomsResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IVenuePhotoRepository _venuePhotoRepository;
        private readonly IUserContext _userContext;

        public GetRecommendedMatchRoomsQueryHandler(
            IMatchRoomRepository matchRoomRepository,
            IFriendshipRepository friendshipRepository,
            IVenuePhotoRepository venuePhotoRepository,
            IUserContext userContext)
        {
            _matchRoomRepository = matchRoomRepository;
            _friendshipRepository = friendshipRepository;
            _venuePhotoRepository = venuePhotoRepository;
            _userContext = userContext;
        }

        public async Task<Result<GetRecommendedMatchRoomsResponse>> Handle(GetRecommendedMatchRoomsQuery request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;

            // Lấy danh sách ID bạn bè
            var friendIds = await _friendshipRepository.GetFriendIdsAsync(userId, cancellationToken);
            if (!friendIds.Any())
            {
                return Result.Success(new GetRecommendedMatchRoomsResponse(
                    new List<RecommendedMatchRoomItemDto>(),
                    0,
                    request.Page,
                    request.PageSize,
                    0
                ));
            }

            // Gọi Repository để lấy phòng của bạn bè có status = open và user chưa tham gia
            var (rooms, total) = await _matchRoomRepository.GetRecommendedRoomsAsync(
                userId,
                friendIds,
                request.Page,
                request.PageSize,
                cancellationToken
            );

            // Fetch venue photos
            var venueIds = rooms.Where(r => r.Field?.Venue != null).Select(r => r.Field!.Venue.VenueId).Distinct().ToList();
            var venuePhotosDict = new Dictionary<Guid, List<RecommendedVenuePhotoDto>>();
            if (venueIds.Any())
            {
                var photosDict = await _venuePhotoRepository.GetPhotosByVenueIdsAsync(venueIds, cancellationToken);
                foreach (var kvp in photosDict)
                {
                    venuePhotosDict[kvp.Key] = kvp.Value
                        .Select(p => new RecommendedVenuePhotoDto(p.PhotoId, p.PhotoUrl, p.DisplayOrder))
                        .ToList();
                }
            }

            var roomItems = rooms.Select(r =>
            {
                var endTime = r.StartTime.Add(TimeSpan.FromMinutes(r.DurationMinutes));
                var venuePhotos = new List<RecommendedVenuePhotoDto>();
                
                if (r.Field?.Venue != null && venuePhotosDict.ContainsKey(r.Field.Venue.VenueId))
                {
                    venuePhotos = venuePhotosDict[r.Field.Venue.VenueId];
                }

                return new RecommendedMatchRoomItemDto(
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

            var response = new GetRecommendedMatchRoomsResponse(
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