using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Fields.Commands.BlockFieldSlot
{
    public class BlockFieldSlotCommandHandler : ICommandHandler<BlockFieldSlotCommand, BlockFieldSlotResponse>
    {
        private const string OFFLINE_BLOCK_REFERENCE = "OFFLINE_BLOCK";

        private readonly IFieldRepository _fieldRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BlockFieldSlotCommandHandler> _logger;

        public BlockFieldSlotCommandHandler(
            IFieldRepository fieldRepository,
            IVenueRepository venueRepository,
            IMatchRoomRepository matchRoomRepository,
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork,
            ILogger<BlockFieldSlotCommandHandler> logger)
        {
            _fieldRepository = fieldRepository;
            _venueRepository = venueRepository;
            _matchRoomRepository = matchRoomRepository;
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<BlockFieldSlotResponse>> Handle(
            BlockFieldSlotCommand request, 
            CancellationToken cancellationToken)
        {
            // Validate EndTime > StartTime (also validated in validator, but double-check)
            if (request.EndTime <= request.StartTime)
            {
                return Result.Failure<BlockFieldSlotResponse>(BlockSlotErrors.InvalidTimeRange);
            }

            // Get venue and verify ownership
            var venue = await _venueRepository.GetVenueWithDetailsAsync(request.VenueId, cancellationToken);
            if (venue == null)
            {
                return Result.Failure<BlockFieldSlotResponse>(BlockSlotErrors.VenueNotFound);
            }

            // Check if user is the venue owner
            if (venue.OwnerId != request.UserId)
            {
                return Result.Failure<BlockFieldSlotResponse>(BlockSlotErrors.Unauthorized);
            }

            // Get field and verify it belongs to the venue
            var field = await _fieldRepository.GetFieldWithVenueAsync(request.FieldId, cancellationToken);
            if (field == null)
            {
                return Result.Failure<BlockFieldSlotResponse>(BlockSlotErrors.FieldNotFound);
            }

            if (field.VenueId != request.VenueId)
            {
                return Result.Failure<BlockFieldSlotResponse>(BlockSlotErrors.FieldNotBelongToVenue);
            }

            // Check if venue is open on this day
            var dayOfWeek = (DayOfWeekEnum)request.Date.DayOfWeek;
            var operatingHour = venue.VenueOperatingHours
                .FirstOrDefault(oh => oh.DayOfWeek == dayOfWeek);

            if (operatingHour == null || operatingHour.IsClosed)
            {
                return Result.Failure<BlockFieldSlotResponse>(BlockSlotErrors.VenueClosedOnDay);
            }

            // Check if time slot is within operating hours
            var openTime = operatingHour.OpenTime ?? TimeSpan.Zero;
            var closeTime = operatingHour.CloseTime ?? TimeSpan.Zero;

            if (request.StartTime < openTime || request.EndTime > closeTime)
            {
                return Result.Failure<BlockFieldSlotResponse>(BlockSlotErrors.OutsideOperatingHours);
            }

            // Check if time slot is available
            var isAvailable = await _bookingRepository.IsTimeSlotAvailableAsync(
                request.FieldId,
                request.Date,
                request.StartTime,
                request.EndTime,
                cancellationToken);

            if (!isAvailable)
            {
                return Result.Failure<BlockFieldSlotResponse>(BlockSlotErrors.SlotAlreadyBooked);
            }

            try
            {
                // Calculate duration
                var durationMinutes = (int)(request.EndTime - request.StartTime).TotalMinutes;

                // Step 1: Create Ghost MatchRoom
                var ghostRoom = new MatchRoom
                {
                    RoomId = Guid.NewGuid(),
                    HostId = request.UserId, // Owner is the host
                    FieldId = request.FieldId,
                    MatchDate = request.Date,
                    StartTime = request.StartTime,
                    DurationMinutes = durationMinutes,
                    MatchFormat = MatchFormat.FiveVsFive, // Default to satisfy Not-Null
                    MatchType = Domain.Enums.MatchType.Friendly, // Default
                    Visibility = Visibility.Private, // Hidden from public search
                    Status = RoomStatus.Locked, // Prevent others from joining
                    Description = request.Reason, // Store blocking reason
                    TotalSlots = 10, // Default for 5v5
                    FilledSlots = 10, // Mark as full immediately
                    CreatedAt = DateTime.UtcNow
                };

                await _matchRoomRepository.AddAsync(ghostRoom);

                // Step 2: Create Ghost Booking
                var ghostBooking = new Booking
                {
                    BookingId = Guid.NewGuid(),
                    RoomId = ghostRoom.RoomId,
                    FieldId = request.FieldId,
                    BookingDate = request.Date,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    TotalAmount = request.Amount,
                    PlatformFee = 0, // No platform fee for offline blocks
                    VenueAmount = request.Amount, // All goes to venue
                    Status = BookingStatus.Confirmed,
                    TransactionReference = OFFLINE_BLOCK_REFERENCE,
                    CreatedAt = DateTime.UtcNow
                };

                await _bookingRepository.AddAsync(ghostBooking);

                // Step 3: Save all changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Field slot blocked: FieldId={FieldId}, Date={Date}, Time={StartTime}-{EndTime}, Reason={Reason}, BlockedBy={UserId}",
                    request.FieldId, request.Date, request.StartTime, request.EndTime, request.Reason, request.UserId);

                return Result.Success(new BlockFieldSlotResponse(
                    ghostRoom.RoomId,
                    ghostBooking.BookingId,
                    request.FieldId,
                    request.Date,
                    request.StartTime,
                    request.EndTime,
                    request.Reason,
                    request.Amount,
                    OFFLINE_BLOCK_REFERENCE,
                    ghostRoom.CreatedAt
                ));
            }
            catch (Exception ex)
            {
                // Handle PostgreSQL exclusion constraint violation (23P01)
                if (ex.InnerException?.Message.Contains("23P01") == true ||
                    ex.Message.Contains("exclusion") ||
                    ex.Message.Contains("overlaps"))
                {
                    _logger.LogWarning(ex, "Slot overlap detected when blocking field slot");
                    return Result.Failure<BlockFieldSlotResponse>(BlockSlotErrors.SlotAlreadyBooked);
                }

                _logger.LogError(ex, "Error blocking field slot");
                throw;
            }
        }
    }
}
