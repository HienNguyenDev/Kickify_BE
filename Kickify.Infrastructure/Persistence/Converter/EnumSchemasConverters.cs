using Kickify.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Infrastructure.Persistence.Converter
{
    public static class EnumSchemasConverters
    {
        // 1. Converter cho Gender (Nullable) for Gender to handle case-insensitive string values
        public static readonly ValueConverter<Gender?, string> GenderConverter = new(
            v => v.HasValue ? v.Value.ToString().ToLower() : null, // Convert enum to lowercase string
            v => ConvertToGender(v));                              // Convert string to enum (case-insensitive)

        // 2. Converter cho UserRole (Not Nullable) for UserRole to handle case-insensitive string values
        public static readonly ValueConverter<UserRole, string> UserRoleConverter = new(
            v => v.ToString().ToLower(),                            // Convert enum to lowercase string for database
            v => ConvertToUserRole(v));                             // Convert string from database to enum (case-insensitive)

        private static Gender? ConvertToGender(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.ToLower() switch
            {
                "male" or "m" => Gender.Male,
                "female" or "f" => Gender.Female,
                "other" => Gender.Other,
                _ => Enum.TryParse<Gender>(value, true, out var gender) ? gender : null
            };
        }

        private static UserRole ConvertToUserRole(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return UserRole.Player;

            return value.ToLower() switch
            {
                "player" or "user" => UserRole.Player,
                "venueowner" or "owner" => UserRole.VenueOwner,
                "admin" => UserRole.Admin,
                _ => Enum.TryParse<UserRole>(value, true, out var role) ? role : UserRole.Player
            };
        }
    }
}
