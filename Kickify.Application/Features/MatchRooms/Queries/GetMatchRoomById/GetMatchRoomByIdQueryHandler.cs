using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.MatchRooms.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRoomById
{
    public class GetMatchRoomByIdQueryHandler : IQueryHandler<GetMatchRoomByIdQuery, GetMatchRoomByIdResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IMatchFormationRepository _matchFormationRepository;

        public GetMatchRoomByIdQueryHandler(
            IMatchRoomRepository matchRoomRepository,
            IMatchFormationRepository matchFormationRepository)
        {
            _matchRoomRepository = matchRoomRepository;
            _matchFormationRepository = matchFormationRepository;
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

            // Map Participants and group by TeamAssignment
            var allParticipants = room.RoomParticipants.Select(p => new RoomParticipantDto(
                p.ParticipantId,
                p.UserId,
                p.User.FullName ?? "Unknown",
                p.User.AvatarUrl,
                p.TeamAssignment.ToString(),
                p.Position,
                p.DepositPaid,
                p.CheckedIn,
                p.CheckInTime,
                p.IsCaptain,
                p.JoinDate
            )).ToList();

            // RULE: Calculate totalDepositCollected from participants with depositPaid = true
            var calculatedTotalDeposit = room.RoomParticipants
                .Where(p => p.DepositPaid && p.DepositAmount.HasValue)
                .Sum(p => p.DepositAmount!.Value);

            var participantsDto = new RoomParticipantsDto(
                TeamA: allParticipants.Where(p => p.TeamAssignment == TeamAssignment.A.ToString()).ToList(),
                TeamB: allParticipants.Where(p => p.TeamAssignment == TeamAssignment.B.ToString()).ToList(),
                Unassigned: allParticipants.Where(p => p.TeamAssignment == TeamAssignment.Unassigned.ToString()).ToList()
            );

            // Map Formations (always include team names even if no formation set)
            var formations = await _matchFormationRepository.GetFormationsByRoomAsync(request.RoomId, cancellationToken);
            
            var teamAFormation = formations.FirstOrDefault(f => f.TeamAssignment == TeamAssignment.A);
            var teamBFormation = formations.FirstOrDefault(f => f.TeamAssignment == TeamAssignment.B);

            // Build Team A DTO - include team name even if no formation
            RoomTeamFormationDto? teamADto = null;
            if (room.TeamAName != null || teamAFormation != null)
            {
                teamADto = new RoomTeamFormationDto(
                    room.TeamAName,
                    teamAFormation?.FormationName,
                    teamAFormation?.Assignments.Select(a => new FormationAssignmentDto(
                        a.PlayerId,
                        a.Player?.FullName ?? "Unknown",
                        a.SlotId,
                        FormationRuleService.GetPositionFromSlotId(a.SlotId)
                    )).ToList() ?? new List<FormationAssignmentDto>()
                );
            }

            // Build Team B DTO - include team name even if no formation
            RoomTeamFormationDto? teamBDto = null;
            if (room.TeamBName != null || teamBFormation != null)
            {
                teamBDto = new RoomTeamFormationDto(
                    room.TeamBName,
                    teamBFormation?.FormationName,
                    teamBFormation?.Assignments.Select(a => new FormationAssignmentDto(
                        a.PlayerId,
                        a.Player?.FullName ?? "Unknown",
                        a.SlotId,
                        FormationRuleService.GetPositionFromSlotId(a.SlotId)
                    )).ToList() ?? new List<FormationAssignmentDto>()
                );
            }

            // Only create formationsDto if at least one team has data
            RoomFormationsDto? formationsDto = null;
            if (teamADto != null || teamBDto != null)
            {
                formationsDto = new RoomFormationsDto(teamADto, teamBDto);
            }

            var response = new GetMatchRoomByIdResponse(
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
                room.TotalDepositCollected,
                //calculatedTotalDeposit, // Use calculated value instead of room.TotalDepositCollected
                room.Visibility.ToString(),
                room.Visibility == Visibility.Private,
                room.Status.ToString(),
                participantsDto,
                formationsDto,
                room.CreatedAt
            );

            return Result.Success(response);
        }
    }
}
