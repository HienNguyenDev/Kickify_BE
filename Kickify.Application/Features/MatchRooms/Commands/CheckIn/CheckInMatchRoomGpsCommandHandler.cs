using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Domain.Common;
using Kickify.Application.Common.Utilities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Kickify.Application.Abstractions.Persistence;

namespace Kickify.Application.Features.MatchRooms.Commands.CheckIn;

public class CheckInMatchRoomGpsCommandHandler : ICommandHandler<CheckInMatchRoomGpsCommand, CheckInMatchRoomResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public CheckInMatchRoomGpsCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _matchRoomRepository = matchRoomRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<CheckInMatchRoomResponse>> Handle(CheckInMatchRoomGpsCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;

        var room = await _matchRoomRepository.GetRoomWithParticipantsForUpdateAsync(request.RoomId, cancellationToken);
        if (room == null)
        {
            return Result.Failure<CheckInMatchRoomResponse>(MatchRoomErrors.NotFound(request.RoomId));
        }

        if (room.Status == RoomStatus.Open || room.Status == RoomStatus.Cancelled)
        {
            return Result.Failure<CheckInMatchRoomResponse>(MatchRoomErrors.RoomNotLockedForCheckIn);
        }

        var participant = room.RoomParticipants.FirstOrDefault(p => p.UserId == currentUserId);
        if (participant == null)
        {
            return Result.Failure<CheckInMatchRoomResponse>(MatchRoomErrors.NotParticipant);
        }

        if (participant.CheckInMethod != null)
        {
            return Result.Success(new CheckInMatchRoomResponse
            {
                Success = true,
                Message = "Already checked in.",
                CheckInMethod = participant.CheckInMethod ?? "Unknown"
            });
        }

        if (room.Field?.Venue?.Latitude == null || room.Field?.Venue?.Longitude == null)
        {
            return Result.Failure<CheckInMatchRoomResponse>(MatchRoomErrors.VenueLocationMissing);
        }

        double distance = GeoCalculator.GetDistanceInMeters(
            request.Latitude,
            request.Longitude,
            room.Field.Venue.Latitude.Value,
            room.Field.Venue.Longitude.Value);

        if (distance > 300)
        {
            return Result.Failure<CheckInMatchRoomResponse>(MatchRoomErrors.TooFarFromVenueGpsOnly(distance));
        }

        participant.CheckedIn = true;
        participant.CheckInTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        participant.CheckInMethod = "GPS";
        participant.CheckInLatitude = request.Latitude;
        participant.CheckInLongitude = request.Longitude;
        participant.DistanceFromVenueMeters = distance;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CheckInMatchRoomResponse
        {
            Success = true,
            Message = "Check-in successful.",
            DistanceMeters = distance,
            CheckInMethod = "GPS"
        });
    }
}
