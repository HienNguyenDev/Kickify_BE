using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRooms
{
    public class GetMatchRoomsQueryHandler : IQueryHandler<GetMatchRoomsQuery, GetMatchRoomsResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;

        public GetMatchRoomsQueryHandler(IMatchRoomRepository matchRoomRepository)
        {
            _matchRoomRepository = matchRoomRepository;
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

            var roomItems = rooms.Select(r =>
            {
                var endTime = r.StartTime.Add(TimeSpan.FromMinutes(r.DurationMinutes));

                return new MatchRoomItemDto(
                    r.RoomId,
                    r.HostId,
                    r.Host.FullName ?? "Unknown",
                    r.Host.AvatarUrl,
                    r.FieldId,
                    r.Field?.FieldName,
                    r.Field?.Venue.VenueName,
                    r.Field?.Venue.Address,
                    r.MatchDate,
                    r.StartTime,
                    endTime,
                    r.DurationMinutes,
                    r.MatchFormat.ToString(),
                    r.TotalSlots,
                    r.FilledSlots,
                    r.DepositPerPerson,
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
