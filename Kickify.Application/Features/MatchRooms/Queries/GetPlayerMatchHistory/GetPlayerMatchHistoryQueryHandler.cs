using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.MatchRooms.Queries.GetPlayerMatchHistory
{
    public class GetPlayerMatchHistoryQueryHandler : IQueryHandler<GetPlayerMatchHistoryQuery, GetPlayerMatchHistoryResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;

        public GetPlayerMatchHistoryQueryHandler(IMatchRoomRepository matchRoomRepository)
        {
            _matchRoomRepository = matchRoomRepository;
        }

        public async Task<Result<GetPlayerMatchHistoryResponse>> Handle(GetPlayerMatchHistoryQuery request, CancellationToken cancellationToken)
        {
            var (rooms, total) = await _matchRoomRepository.GetMatchHistoryByUserAsync(
                request.TargetUserId,
                request.Page,
                request.PageSize,
                cancellationToken
            );

            var roomItems = rooms.Select(room =>
            {
                var endTime = room.StartTime.Add(TimeSpan.FromMinutes(room.DurationMinutes));

                var hostDto = new PlayerRoomHostDto(
                    room.Host.UserId,
                    room.Host.FullName ?? "Unknown",
                    room.Host.AvatarUrl
                );

                PlayerRoomFieldDto? fieldDto = null;
                if (room.Field != null)
                {
                    var venueDto = new PlayerRoomVenueDto(
                        room.Field.Venue.VenueId,
                        room.Field.Venue.VenueName
                    );

                    fieldDto = new PlayerRoomFieldDto(
                        room.Field.FieldId,
                        room.Field.FieldName,
                        venueDto
                    );
                }

                return new PlayerMatchRoomItemDto(
                    room.RoomId,
                    room.HostId,
                    hostDto,
                    room.FieldId,
                    fieldDto,
                    room.RoomName,
                    room.MatchDate,
                    room.StartTime,
                    endTime,
                    room.DurationMinutes,
                    room.MatchFormat.ToString(),
                    room.Description,
                    room.TeamAScore,
                    room.TeamBScore,
                    room.Status.ToString(),
                    room.CreatedAt
                );
            }).ToList();

            var response = new GetPlayerMatchHistoryResponse(
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
