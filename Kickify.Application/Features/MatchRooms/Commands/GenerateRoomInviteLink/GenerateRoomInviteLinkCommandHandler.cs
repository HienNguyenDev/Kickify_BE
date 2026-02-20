using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.MatchRooms.Commands.GenerateRoomInviteLink;

public class GenerateRoomInviteLinkCommandHandler : ICommandHandler<GenerateRoomInviteLinkCommand, GenerateRoomInviteLinkResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IRoomInvitationRepository _roomInvitationRepository;
    private readonly IQrCodeService _qrCodeService;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public GenerateRoomInviteLinkCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IRoomInvitationRepository roomInvitationRepository,
        IQrCodeService qrCodeService,
        IStorageService storageService,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _matchRoomRepository = matchRoomRepository;
        _roomInvitationRepository = roomInvitationRepository;
        _qrCodeService = qrCodeService;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<GenerateRoomInviteLinkResponse>> Handle(GenerateRoomInviteLinkCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        var room = await _matchRoomRepository.GetRoomWithParticipantsAsync(request.RoomId, cancellationToken);
        if (room is null)
            return Result.Failure<GenerateRoomInviteLinkResponse>(MatchRoomErrors.NotFound(request.RoomId));

        // Verify user is a participant of the room
        var isParticipant = room.RoomParticipants.Any(p => p.UserId == userId);
        if (!isParticipant)
            return Result.Failure<GenerateRoomInviteLinkResponse>(MatchRoomErrors.NotParticipant);

        // Only allow generating invite links for Open rooms
        if (room.Status != RoomStatus.Open)
            return Result.Failure<GenerateRoomInviteLinkResponse>(MatchRoomErrors.NotOpen);

        var deepLink = $"kickify://room/{room.RoomId}";
        var webLink = $"https://kickify.app/room/{room.RoomId}";

        // Generate QR code image containing the deep link
        var qrCodeBytes = _qrCodeService.GenerateQrCodePng(deepLink);

        // Upload QR code image to storage
        var fileName = $"qrcodes/room-invite-{room.RoomId}-{Guid.NewGuid():N}.png";
        using var stream = new MemoryStream(qrCodeBytes);
        var uploadResult = await _storageService.UploadAsync(stream, fileName, "image/png", cancellationToken);

        if (!uploadResult.Success)
            return Result.Failure<GenerateRoomInviteLinkResponse>(
                new Error("MatchRoom.QrCodeUploadFailed", uploadResult.ErrorMessage ?? "Failed to upload QR code image", ErrorType.Failure));

        var invitation = new RoomInvitation
        {
            InvitationId = Guid.NewGuid(),
            RoomId = room.RoomId,
            InviterId = userId,
            InviteeId = Guid.Empty, // QR/link invitation - no specific invitee
            InvitationLink = webLink,
            QrCodeUrl = uploadResult.PublicUrl,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _roomInvitationRepository.AddAsync(invitation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new GenerateRoomInviteLinkResponse(
            invitation.InvitationId,
            room.RoomId,
            deepLink,
            webLink,
            uploadResult.PublicUrl,
            invitation.CreatedAt
        ));
    }
}
