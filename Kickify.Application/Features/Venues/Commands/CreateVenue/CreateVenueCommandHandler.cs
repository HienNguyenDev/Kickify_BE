using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using MediatR;

namespace Kickify.Application.Features.Venues.Commands.CreateVenue;

public class CreateVenueCommandHandler : IRequestHandler<CreateVenueCommand, Result<CreateVenueResponse>>
{
    private readonly IVenueRepository _venueRepository;
    private readonly IVenueWalletRepository _venueWalletRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVenueCommandHandler(IVenueRepository venueRepository, IVenueWalletRepository venueWalletRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _venueRepository = venueRepository;
        _venueWalletRepository = venueWalletRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateVenueResponse>> Handle(CreateVenueCommand request, CancellationToken cancellationToken)
    {
        // Verify owner exists
        var owner = await _userRepository.GetByIdAsync(request.OwnerId);
        if (owner == null)
        {
            return Result.Failure<CreateVenueResponse>(UserErrors.NotFound(request.OwnerId));
        }

        // Begin transaction for atomic Venue + Wallet + Fields + OperatingHours creation
        try
        {
            // 1. Create Venue
            var venue = new Venue
            {
                VenueId = Guid.NewGuid(),
                OwnerId = request.OwnerId,
                VenueName = request.Name,
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _venueRepository.AddAsync(venue);

            // 2. Auto-create VenueWallet with initial balance = 0
            var wallet = new VenueWallet
            {
                VenueWalletId = Guid.NewGuid(),
                VenueId = venue.VenueId,
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _venueWalletRepository.AddAsync(wallet);

            // 3. Create Fields
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
                    HourlyRate = fieldDto.PricePerHour,
                    CreatedAt = DateTime.UtcNow
                };

                venue.Fields.Add(field);
            }

            // 4. Create OperatingHours
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

            // Save all changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateVenueResponse(
                venue.VenueId,
                venue.VenueName,
                venue.Address,
                venue.Latitude ?? 0,
                venue.Longitude ?? 0,
                venue.Description,
                wallet.VenueWalletId,
                venue.Fields.Select(f => new VenueFieldDto(f.FieldId, f.FieldName, f.FieldType.ToString(), 0, f.HourlyRate, null)).ToList(),
                venue.CreatedAt
            );

            return Result.Success(response);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
