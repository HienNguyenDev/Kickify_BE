using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRoomById
{
    public class GetMatchRoomByIdQueryHandler : IQueryHandler<GetMatchRoomByIdQuery, GetMatchRoomByIdResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;

        public GetMatchRoomByIdQueryHandler(IMatchRoomRepository matchRoomRepository)
        {
            _matchRoomRepository = matchRoomRepository;
        }

        public async Task<Result<GetMatchRoomByIdResponse>> Handle(GetMatchRoomByIdQuery request, CancellationToken cancellationToken)
        {
            var room = await _matchRoomRepository.GetRoomWithDetailsAsync(request.RoomId, cancellationToken);

            if (room == null)
            {
                return Result.Failure<GetMatchRoomByIdResponse>(MatchRoomErrors.NotFound(request.RoomId));
            }

            // Calculate EndTime
            var endTime = room.StartTime.Add(TimeSpan.FromMinutes(room.DurationMinutes));

            // Map Host
            var hostDto = new RoomHostDto(
                room.Host.UserId,
                room.Host.FullName ?? "Unknown",
                room.Host.AvatarUrl
            );

            // Map Field and Venue
            RoomFieldDto? fieldDto = null;
            if (room.Field != null)
            {
                var venueDto = new RoomVenueDto(
                    room.Field.Venue.VenueId,
                    room.Field.Venue.VenueName,
                    room.Field.Venue.Address,
                    room.Field.Venue.ContactPhone
                );

                fieldDto = new RoomFieldDto(
                    room.Field.FieldId,
                    room.Field.FieldName,
                    room.Field.FieldType.ToString(),
                    room.Field.HourlyRate,
                    venueDto
                );
            }

            // Map Participants
            var participants = room.RoomParticipants.Select(p => new RoomParticipantDto(
                p.ParticipantId,
                p.UserId,
                p.User.FullName ?? "Unknown",
                p.User.AvatarUrl,
                p.TeamAssignment.ToString(),
                p.Position,
                p.DepositPaid,
                p.JoinDate
            )).ToList();

            var response = new GetMatchRoomByIdResponse(
                room.RoomId,
                room.HostId,
                hostDto,
                room.FieldId,
                fieldDto,
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
                room.TotalDepositCollected,
                room.Status.ToString(),
                participants,
                room.CreatedAt
            );

            return Result.Success(response);
        }
    }
}
