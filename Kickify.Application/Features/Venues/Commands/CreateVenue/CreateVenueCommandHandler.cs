using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Venues.Commands.CreateVenue;

public class CreateVenueCommandHandler : ICommandHandler<CreateVenueCommand, CreateVenueResponse>
{
    private readonly IVenueRepository _venueRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly ILogger<CreateVenueCommandHandler> _logger;

    public CreateVenueCommandHandler(
        IVenueRepository venueRepository,
        IHolidayRepository holidayRepository,
        IWalletRepository walletRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        ILogger<CreateVenueCommandHandler> logger)
    {
        _venueRepository = venueRepository;
        _holidayRepository = holidayRepository;
        _walletRepository = walletRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<Result<CreateVenueResponse>> Handle(CreateVenueCommand request, CancellationToken cancellationToken)
    {
        var ownerId = _userContext.UserId;

        var owner = await _userRepository.GetByIdAsync(ownerId);
        if (owner == null)
        {
            return Result.Failure<CreateVenueResponse>(UserErrors.NotFound(ownerId));
        }

        try
        {
            var venue = new Venue
            {
                VenueId = Guid.NewGuid(),
                OwnerId = ownerId,
                VenueName = request.Name,
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ContactPhone = request.ContactPhone,
                ContactEmail = request.ContactEmail,
                Description = request.Description,
                Amenities = request.Amenities,
                Status = VenueStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            var ignoredHolidayIds = request.IgnoredHolidayIds.Distinct().ToList();
            if (ignoredHolidayIds.Count > 0)
            {
                var holidays = await _holidayRepository.GetByIdsAsync(ignoredHolidayIds, cancellationToken);
                if (holidays.Count != ignoredHolidayIds.Count)
                {
                    var missingHolidayIds = ignoredHolidayIds.Except(holidays.Select(h => h.Id)).ToList();
                    return Result.Failure<CreateVenueResponse>(HolidayErrors.InvalidIds(missingHolidayIds));
                }

                await _venueRepository.SyncIgnoredHolidaysAsync(venue, holidays, cancellationToken);
            }

            await _venueRepository.AddAsync(venue);

            var wallet = await _walletRepository.GetByUserIdAsync(ownerId, cancellationToken);
            Guid walletId;
            if (wallet is null)
            {
                wallet = new Wallet
                {
                    WalletId = Guid.NewGuid(),
                    UserId = ownerId,
                    WalletType = WalletType.VenueOwner,
                    Balance = 0
                };
                await _walletRepository.AddAsync(wallet);
            }
            walletId = wallet.WalletId;

            foreach (var fieldDto in request.Fields)
            {
                if (!Enum.TryParse<FieldType>(fieldDto.FieldType, true, out var fieldType))
                {
                    return Result.Failure<CreateVenueResponse>(VenueErrors.InvalidFieldType(fieldDto.FieldType));
                }

                var field = new Field
                {
                    FieldId = Guid.NewGuid(),
                    VenueId = venue.VenueId,
                    FieldName = fieldDto.Name,
                    FieldType = fieldType,
                    SurfaceType = fieldDto.SurfaceType,
                    HourlyRate = fieldDto.HourlyRate,
                    PeakHourSurcharge = fieldDto.PeakHourSurcharge,
                    PeakStartTime = fieldDto.PeakStartTime,
                    PeakEndTime = fieldDto.PeakEndTime,
                    WeekendSurcharge = fieldDto.WeekendSurcharge,
                    HolidaySurcharge = fieldDto.HolidaySurcharge,
                    CreatedAt = DateTime.UtcNow
                };

                venue.Fields.Add(field);
            }

            foreach (var ohDto in request.OperatingHours)
            {
                var operatingHour = new VenueOperatingHour
                {
                    HoursId = Guid.NewGuid(),
                    VenueId = venue.VenueId,
                    DayOfWeek = (DayOfWeekEnum)ohDto.DayOfWeek,
                    OpenTime = ohDto.OpenTime,
                    CloseTime = ohDto.CloseTime,
                    IsClosed = false
                };

                venue.VenueOperatingHours.Add(operatingHour);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Venue {VenueId} created with {FieldCount} fields", venue.VenueId, venue.Fields.Count);

            return Result.Success(new CreateVenueResponse(
                venue.VenueId,
                venue.VenueName,
                venue.Address,
                venue.Latitude ?? 0,
                venue.Longitude ?? 0,
                venue.ContactPhone,
                venue.ContactEmail,
                venue.Description,
                venue.Amenities,
                walletId,
                venue.Fields.Select(f => new VenueFieldDto(
                    f.FieldId,
                    f.FieldName,
                    f.FieldType.ToString(),
                    f.SurfaceType,
                    f.HourlyRate,
                    f.PeakHourSurcharge,
                    f.PeakStartTime,
                    f.PeakEndTime,
                    f.WeekendSurcharge,
                    f.HolidaySurcharge
                )).ToList(),
                venue.CreatedAt
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating venue");
            throw;
        }
    }
}