using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.MatchRooms.Queries.GetMyMatchRooms
{
    public class GetMyMatchRoomsQueryHandler : IQueryHandler<GetMyMatchRoomsQuery, GetMyMatchRoomsResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IMatchFeedbackRepository _matchFeedbackRepository;
        private readonly IUserContext _userContext;

        public GetMyMatchRoomsQueryHandler(
            IMatchRoomRepository matchRoomRepository,
            IMatchFeedbackRepository matchFeedbackRepository,
            IUserContext userContext)
        {
            _matchRoomRepository = matchRoomRepository;
            _matchFeedbackRepository = matchFeedbackRepository;
            _userContext = userContext;
        }

        public async Task<Result<GetMyMatchRoomsResponse>> Handle(GetMyMatchRoomsQuery request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;

            var (rooms, total) = await _matchRoomRepository.GetRoomsByUserAsync(
                userId,
                request.Page,
                request.PageSize,
                cancellationToken
            );

            var matchIds = rooms.Select(r => r.RoomId).ToList();
            var reviewedMatchIds = matchIds.Any()
                ? await _matchFeedbackRepository.GetMatchesReviewedByUserAsync(userId, matchIds, cancellationToken)
                : new List<Guid>();

            var roomItems = rooms.Select(room =>
            {
                var endTime = room.StartTime.Add(TimeSpan.FromMinutes(room.DurationMinutes));

                // Map Host
                var hostDto = new MyRoomHostDto(
                    room.Host.UserId,
                    room.Host.FullName ?? "Unknown",
                    room.Host.AvatarUrl
                );

                // Map Field and Venue
                MyRoomFieldDto? fieldDto = null;
                if (room.Field != null)
                {
                    var venueDto = new MyRoomVenueDto(
                        room.Field.Venue.VenueId,
                        room.Field.Venue.VenueName,
                        room.Field.Venue.Address,
                        room.Field.Venue.ContactPhone
                    );

                    fieldDto = new MyRoomFieldDto(
                        room.Field.FieldId,
                        room.Field.FieldName,
                        room.Field.FieldType.ToString(),
                        room.Field.HourlyRate,
                        venueDto
                    );
                }

                // Calculate totalDepositCollected from participants with depositPaid = true
                var calculatedTotalDeposit = room.RoomParticipants
                    .Where(p => p.DepositPaid && p.DepositAmount.HasValue)
                    .Sum(p => p.DepositAmount!.Value);

                return new MyMatchRoomItemDto(
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
                    room.Rules,
                    room.TotalSlots,
                    room.FilledSlots,
                    room.DepositPerPerson,
                    calculatedTotalDeposit,
                    room.Visibility.ToString(),
                    room.Visibility == Domain.Enums.Visibility.Private,
                    room.Status.ToString(),
                    room.CreatedAt,
                    reviewedMatchIds.Contains(room.RoomId)
                );
            }).ToList();

            var response = new GetMyMatchRoomsResponse(
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
