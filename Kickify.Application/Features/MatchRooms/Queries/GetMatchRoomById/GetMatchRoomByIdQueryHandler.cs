using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.MatchRooms.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRoomById
{
    public class GetMatchRoomByIdQueryHandler : IQueryHandler<GetMatchRoomByIdQuery, GetMatchRoomByIdResponse>
    {
        private const decimal SkillImbalanceEloThreshold = 200m;

        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IMatchFormationRepository _matchFormationRepository;
        private readonly IMatchFeedbackRepository _matchFeedbackRepository;

        public GetMatchRoomByIdQueryHandler(
            IMatchRoomRepository matchRoomRepository,
            IMatchFormationRepository matchFormationRepository,
            IMatchFeedbackRepository matchFeedbackRepository)
        {
            _matchRoomRepository = matchRoomRepository;
            _matchFormationRepository = matchFormationRepository;
            _matchFeedbackRepository = matchFeedbackRepository;
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

            // Fetch feedback state
            var roomFeedbacks = await _matchFeedbackRepository.GetFeedbacksByMatchAsync(request.RoomId, cancellationToken);
            var usersWhoLeftFeedback = roomFeedbacks.Select(f => f.ReviewerId).ToHashSet();

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
                p.JoinDate,
                usersWhoLeftFeedback.Contains(p.UserId)
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

            var teamAPlayers = room.RoomParticipants.Where(p => p.TeamAssignment == TeamAssignment.A).ToList();
            var teamBPlayers = room.RoomParticipants.Where(p => p.TeamAssignment == TeamAssignment.B).ToList();
            var teamAAverageElo = ComputeTeamAverageElo(teamAPlayers);
            var teamBAverageElo = ComputeTeamAverageElo(teamBPlayers);
            var isSkillImbalanced = teamAPlayers.Count > 0
                && teamBPlayers.Count > 0
                && decimal.Abs(teamAAverageElo - teamBAverageElo) > SkillImbalanceEloThreshold;

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
                teamAAverageElo,
                teamBAverageElo,
                isSkillImbalanced,
                room.CreatedAt
            );

            return Result.Success(response);
        }

        /// <summary>Mean CurrentElo for assigned team members; empty team returns 0; missing profile uses default 1000.</summary>
        private static decimal ComputeTeamAverageElo(IReadOnlyList<RoomParticipant> membersOnTeam)
        {
            if (membersOnTeam.Count == 0)
            {
                return 0m;
            }

            decimal sum = 0;
            foreach (var p in membersOnTeam)
            {
                sum += p.User.PlayerProfile?.CurrentElo ?? 1000;
            }

            return Math.Round(sum / membersOnTeam.Count, 1, MidpointRounding.AwayFromZero);
        }
    }
}
