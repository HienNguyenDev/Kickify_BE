using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Bookings.Commands.ProcessPayment
{
    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<ProcessPaymentResponse>>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IRoomParticipantRepository _roomParticipantRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly IVenueWalletRepository _venueWalletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProcessPaymentCommandHandler> _logger;

        public ProcessPaymentCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IRoomParticipantRepository roomParticipantRepository,
            IBookingRepository bookingRepository,
            IFieldRepository fieldRepository,
            IVenueWalletRepository venueWalletRepository,
            IUnitOfWork unitOfWork,
            ILogger<ProcessPaymentCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _roomParticipantRepository = roomParticipantRepository;
            _bookingRepository = bookingRepository;
            _fieldRepository = fieldRepository;
            _venueWalletRepository = venueWalletRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<ProcessPaymentResponse>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            // Get room with participants
            var room = await _matchRoomRepository.GetRoomWithParticipantsAsync(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure<ProcessPaymentResponse>(
                    new Error("Room.NotFound", "Match room not found", ErrorType.NotFound));
            }

            // Check if user is participant
            var participant = room.RoomParticipants.FirstOrDefault(p => p.UserId == request.UserId);
            if (participant == null)
            {
                return Result.Failure<ProcessPaymentResponse>(
                    new Error("Participant.NotFound", "User is not a participant of this room", ErrorType.NotFound));
            }

            // Check if already paid
            if (participant.DepositPaid)
            {
                return Result.Failure<ProcessPaymentResponse>(
                    new Error("Payment.AlreadyPaid", "User has already paid", ErrorType.Conflict));
            }

            // Mark as paid
            participant.DepositPaid = true;
            _roomParticipantRepository.Update(participant);

            // Check if all participants have paid
            bool allPaid = await _matchRoomRepository.AreAllParticipantsPaidAsync(request.RoomId, cancellationToken);

            if (allPaid)
            {
                _logger.LogInformation("All participants paid for room {RoomId}. Creating booking...", request.RoomId);

                // Get field details
                var field = await _fieldRepository.GetFieldWithVenueAsync(room.FieldId!.Value, cancellationToken);
                if (field == null)
                {
                    return Result.Failure<ProcessPaymentResponse>(FieldErrors.NotFound(room.FieldId));
                }

                // Calculate total amount
                var totalAmount = room.RoomParticipants.Sum(p => p.DepositAmount ?? 0);

                try
                {
                    // Create booking - THIS IS WHERE RACE CONDITION CAN HAPPEN
                    var booking = new Booking
                    {
                        BookingId = Guid.NewGuid(),
                        FieldId = room.FieldId.Value,
                        RoomId = request.RoomId,
                        BookingDate = room.MatchDate,
                        StartTime = room.StartTime,
                        EndTime = room.StartTime.Add(TimeSpan.FromMinutes(room.DurationMinutes)),
                        TotalAmount = totalAmount,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _bookingRepository.AddAsync(booking);

                    // Update venue wallet
                    var wallet = await _venueWalletRepository.GetByVenueIdAsync(field.VenueId, cancellationToken);
                    if (wallet != null)
                    {
                        wallet.Balance += totalAmount;
                    }

                    // Save changes - exclusion constraint will be checked here
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Booking {BookingId} created successfully for room {RoomId}", 
                        booking.BookingId, request.RoomId);

                    return Result.Success(new ProcessPaymentResponse(
                        true,
                        "Payment processed successfully. Booking created.",
                        booking.BookingId,
                        booking.BookingDate,
                        booking.StartTime,
                        booking.StartTime.Add(TimeSpan.FromMinutes(room.DurationMinutes))
                    ));
                }
                catch (DbUpdateException ex) when (IsExclusionConstraintViolation(ex))
                {
                    // RACE CONDITION DETECTED: Another room booked this slot first
                    _logger.LogWarning("Race condition detected for room {RoomId}. Field {FieldId} already booked for {Date} {StartTime}-{EndTime}",
                        request.RoomId, room.FieldId, room.MatchDate, room.StartTime, room.StartTime.Add(TimeSpan.FromMinutes(room.DurationMinutes)));

                    return Result.Failure<ProcessPaymentResponse>(BookingErrors.DoubleBooking);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment for room {RoomId}", request.RoomId);
                    throw;
                }
            }
            else
            {
                // Not all paid yet, just save participant status
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User {UserId} paid for room {RoomId}. Waiting for other participants...",
                    request.UserId, request.RoomId);

                return Result.Success(new ProcessPaymentResponse(
                    false,
                    "Payment recorded. Waiting for other participants to pay.",
                    null,
                    null,
                    null,
                    null
                ));
            }
        }

        private bool IsExclusionConstraintViolation(DbUpdateException ex)
        {
            // Check if the exception is caused by PostgreSQL exclusion constraint violation
            // Error code 23P01 = exclusion_violation
            var innerException = ex.InnerException;
            if (innerException != null)
            {
                var message = innerException.Message;
                return message.Contains("23P01") || message.Contains("no_overlap_booking") || message.Contains("exclusion_violation");
            }

            return false;
        }
    }
}
