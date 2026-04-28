using AutoMapper;
using Kickify.Application.Features.Fields.Commands.UpdateField;
using Kickify.Application.Features.Venues.Commands.UpdateVenue;
using Kickify.Domain.Entities;

namespace Kickify.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // UpdateVenueCommand -> Venue
            // Rule: null = keep old value, non-null (including empty string) = update
            CreateMap<UpdateVenueCommand, Venue>()
                .ForMember(dest => dest.VenueName, opt =>
                {
                    opt.Condition((src, dest, srcMember) => src.Name != null);
                    opt.MapFrom(src => src.Name);
                })
                .ForMember(dest => dest.Address, opt => opt.Condition((src, dest, srcMember) => src.Address != null))
                .ForMember(dest => dest.Latitude, opt => opt.Condition((src, dest, srcMember) => src.Latitude.HasValue))
                .ForMember(dest => dest.Longitude, opt => opt.Condition((src, dest, srcMember) => src.Longitude.HasValue))
                .ForMember(dest => dest.ContactPhone, opt => opt.Condition((src, dest, srcMember) => src.ContactPhone != null))
                .ForMember(dest => dest.ContactEmail, opt => opt.Condition((src, dest, srcMember) => src.ContactEmail != null))
                .ForMember(dest => dest.Description, opt => opt.Condition((src, dest, srcMember) => src.Description != null))
                .ForMember(dest => dest.Amenities, opt => opt.Condition((src, dest, srcMember) => src.Amenities != null))
                // Ignore properties that should not be mapped
                .ForMember(dest => dest.VenueId, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.AdminNotes, opt => opt.Ignore())
                .ForMember(dest => dest.AverageRating, opt => opt.Ignore())
                .ForMember(dest => dest.TotalReviews, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.VenuePhotos, opt => opt.Ignore())
                .ForMember(dest => dest.VenueOperatingHours, opt => opt.Ignore())
                .ForMember(dest => dest.Fields, opt => opt.Ignore())
                .ForMember(dest => dest.VenueReviews, opt => opt.Ignore())
                .ForMember(dest => dest.IgnoredHolidays, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // UpdateFieldCommand -> Field
            // Rule: null = keep old value, non-null (including empty string) = update
            CreateMap<UpdateFieldCommand, Field>()
                .ForMember(dest => dest.FieldName, opt => opt.Condition((src, dest, srcMember) => src.FieldName != null))
                .ForMember(dest => dest.SurfaceType, opt => opt.Condition((src, dest, srcMember) => src.SurfaceType != null))
                .ForMember(dest => dest.HourlyRate, opt => opt.Condition((src, dest, srcMember) => src.HourlyRate.HasValue))
                .ForMember(dest => dest.WeekendSurcharge, opt => opt.Condition((src, dest, srcMember) => src.WeekendSurcharge.HasValue))
                .ForMember(dest => dest.HolidaySurcharge, opt => opt.Condition((src, dest, srcMember) => src.HolidaySurcharge.HasValue))
                .ForMember(dest => dest.IsActive, opt => opt.Condition((src, dest, srcMember) => src.IsActive.HasValue))
                // Special handling for FieldType enum - handled manually in handler
                .ForMember(dest => dest.FieldType, opt => opt.Ignore())
                .ForMember(dest => dest.PeakHours, opt => opt.Ignore())
                .ForMember(dest => dest.IsWeekendSurchargePercentage, opt => opt.Ignore())
                .ForMember(dest => dest.IsHolidaySurchargePercentage, opt => opt.Ignore())
                // Ignore properties that should not be mapped
                .ForMember(dest => dest.FieldId, opt => opt.Ignore())
                .ForMember(dest => dest.VenueId, opt => opt.Ignore())
                .ForMember(dest => dest.Venue, opt => opt.Ignore())
                .ForMember(dest => dest.MatchRooms, opt => opt.Ignore())
                .ForMember(dest => dest.MatchPresets, opt => opt.Ignore())
                .ForMember(dest => dest.Bookings, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}
