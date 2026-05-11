using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Domain.Common;
using Kickify.Application.Common.Utilities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Kickify.Application.Abstractions.Persistence; // IUnitOfWork or similarly named interface assuming clean architecture. Wait I'll inject IUnitOfWork.

namespace Kickify.Application.Features.MatchRooms.Commands.CheckIn;

public class CheckInMatchRoomCommandHandler : ICommandHandler<CheckInMatchRoomCommand, CheckInMatchRoomResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
     private readonly IStorageService _storageService;

    public CheckInMatchRoomCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IMatchRoomHubService matchRoomHubService,
        IStorageService storageService)
    {
        _matchRoomRepository = matchRoomRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
         _storageService = storageService;
    }

    public async Task<Result<CheckInMatchRoomResponse>> Handle(CheckInMatchRoomCommand request, CancellationToken cancellationToken)
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
            return Result.Success<CheckInMatchRoomResponse>(new CheckInMatchRoomResponse
            {
                Success = true,
                Message = "Already checked in.",
                CheckInMethod = participant.CheckInMethod ?? "Unknown"
            });
        }

        string? publicPhotoUrl = null;
        double? distance = null;
        string checkInMethod = string.Empty;

        // Mandatory Lat/Lon for both methods
        if (!request.Latitude.HasValue || !request.Longitude.HasValue)
        {
            return Result.Failure<CheckInMatchRoomResponse>(MatchRoomErrors.InvalidCheckInMethod);
        }

        if (room.Field?.Venue?.Latitude == null || room.Field?.Venue?.Longitude == null)
        {
            return Result.Failure<CheckInMatchRoomResponse>(MatchRoomErrors.VenueLocationMissing);
        }

        // Calculate distance first
        distance = GeoCalculator.GetDistanceInMeters(
            request.Latitude.Value,
            request.Longitude.Value,
            room.Field.Venue.Latitude.Value,
            room.Field.Venue.Longitude.Value);

        // Nếu sai số GPS quá mức vô lý (ví dụ > 300m) thì cấm luôn
        // ========================================================
        if (distance > 300)
        {
            return Result.Failure<CheckInMatchRoomResponse>(MatchRoomErrors.WayTooFarEvenForPhoto(distance.Value));
        }

        // Branch 1: Photo Check-in (Fallback)
        if (request.Photo != null)
        {
            
            // Upload to Storage
            string uniqueFileName = $"checkin_{Guid.NewGuid():N}_{request.Photo.FileName}";
            var uploadResult = await _storageService.UploadAsync(
                request.Photo.Stream, 
                uniqueFileName, 
                request.Photo.ContentType, 
                cancellationToken);

            if (!uploadResult.Success)
            {
                return Result.Failure<CheckInMatchRoomResponse>(Error.Failure("UploadError", uploadResult.ErrorMessage ?? "Failed to upload photo"));
            }

            publicPhotoUrl = uploadResult.PublicUrl;
            checkInMethod = "Photo";
           
        }
        // Branch 2: GPS Check-in
        else
        {
            if (distance > 200)
            {
                return Result.Failure<CheckInMatchRoomResponse>(MatchRoomErrors.TooFarFromVenue(distance.Value));
            }
            checkInMethod = "GPS";
        }

        participant.CheckedIn = true;
        participant.CheckInTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        participant.CheckInMethod = checkInMethod;
        participant.CheckInLatitude = request.Latitude.Value;
        participant.CheckInLongitude = request.Longitude.Value;
        participant.DistanceFromVenueMeters = distance;
        participant.CheckInPhotoUrl = publicPhotoUrl;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        int totalParticipants = room.RoomParticipants.Count;
        int checkedInCount = room.RoomParticipants.Count(p => p.CheckedIn);
        bool allCheckedIn = (checkedInCount == totalParticipants);

        

        return Result.Success<CheckInMatchRoomResponse>(new CheckInMatchRoomResponse
        {
            Success = true,
            Message = "Check-in successful.",
            DistanceMeters = distance,
            CheckInMethod = checkInMethod
        });
    }
}

