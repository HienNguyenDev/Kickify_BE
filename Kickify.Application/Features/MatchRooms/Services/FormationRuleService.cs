using Kickify.Domain.Enums;

namespace Kickify.Application.Features.MatchRooms.Services;

public static class FormationRuleService
{
    // Valid formations per match format
    private static readonly Dictionary<MatchFormat, HashSet<string>> ValidFormations = new()
    {
        [MatchFormat.FiveVsFive] = new HashSet<string>
        {
            "3-1",      // 3 defenders, 1 forward (+ 1 GK = 5)
            "2-2",      // 2 defenders, 2 forwards
            "2-1-1",    // 2 defenders, 1 midfielder, 1 forward
            "1-2-1",    // 1 defender, 2 midfielders, 1 forward
            "1-3"       // 1 defender, 3 forwards
        },
        [MatchFormat.SevenVsSeven] = new HashSet<string>
        {
            "3-3",      // 3 defenders, 3 forwards (+ 1 GK = 7)
            "4-2",      // 4 defenders, 2 forwards
            "3-2-1",    // 3 defenders, 2 midfielders, 1 forward
            "3-1-2",    // 3 defenders, 1 midfielder, 2 forwards
            "2-3-1",    // 2 defenders, 3 midfielders, 1 forward
            "4-1-1"     // 4 defenders, 1 midfielder, 1 forward
        },
        [MatchFormat.ElevenVsEleven] = new HashSet<string>
        {
            "4-4-2",    // Classic formation (+ 1 GK = 11)
            "4-3-3",    // Attack-minded
            "3-5-2",    // Wing-back system
            "4-2-3-1",  // Modern formation
            "5-3-2",    // Defensive
            "3-4-3",    // All-out attack
            "4-5-1",    // Very defensive
            "5-4-1"     // Ultra defensive
        }
    };

    /// <summary>
    /// Check if a formation name is valid for the given match format
    /// </summary>
    public static bool IsValidFormation(MatchFormat format, string formationName)
    {
        if (string.IsNullOrWhiteSpace(formationName))
            return false;

        return ValidFormations.TryGetValue(format, out var validFormations) 
            && validFormations.Contains(formationName.Trim());
    }

    /// <summary>
    /// Get all valid formations for a given match format
    /// </summary>
    public static IReadOnlyCollection<string> GetValidFormations(MatchFormat format)
    {
        return ValidFormations.TryGetValue(format, out var validFormations)
            ? validFormations.ToList().AsReadOnly()
            : Array.Empty<string>();
    }

    /// <summary>
    /// Generate expected slot IDs for a formation
    /// Format: GK-0 (Goalkeeper), DF-x (Defender), MF-x (Midfielder), FW-x (Forward)
    /// </summary>
    public static HashSet<string> GenerateExpectedSlots(MatchFormat format, string formationName)
    {
        var slots = new HashSet<string>();
        
        if (!IsValidFormation(format, formationName))
            return slots;

        // Always have 1 goalkeeper
        slots.Add("GK-0");

        var parts = formationName.Split('-').Select(int.Parse).ToArray();

        switch (parts.Length)
        {
            case 2: // e.g., "3-1" or "4-2"
                // First number = defenders, Second number = forwards
                for (int i = 0; i < parts[0]; i++)
                    slots.Add($"DF-{i}");
                for (int i = 0; i < parts[1]; i++)
                    slots.Add($"FW-{i}");
                break;

            case 3: // e.g., "4-4-2" or "3-2-1"
                // Defenders - Midfielders - Forwards
                for (int i = 0; i < parts[0]; i++)
                    slots.Add($"DF-{i}");
                for (int i = 0; i < parts[1]; i++)
                    slots.Add($"MF-{i}");
                for (int i = 0; i < parts[2]; i++)
                    slots.Add($"FW-{i}");
                break;

            case 4: // e.g., "4-2-3-1"
                // Defenders - Defensive Mids - Attacking Mids - Forwards
                // Combine both mid roles into MF slots
                for (int i = 0; i < parts[0]; i++)
                    slots.Add($"DF-{i}");
                int midCount = parts[1] + parts[2];
                for (int i = 0; i < midCount; i++)
                    slots.Add($"MF-{i}");
                for (int i = 0; i < parts[3]; i++)
                    slots.Add($"FW-{i}");
                break;
        }

        return slots;
    }

    /// <summary>
    /// Get the position name from a slot ID (e.g., "DF-1" -> "Defender")
    /// </summary>
    public static string GetPositionFromSlotId(string slotId)
    {
        if (string.IsNullOrWhiteSpace(slotId))
            return "Unknown";

        var prefix = slotId.Split('-')[0].ToUpperInvariant();
        return prefix switch
        {
            "GK" => "GK", //old will be Goalkeeper
            "DF" => "DF", //old will be Defender
            "MF" => "MF", //old will be Midfielder
            "FW" => "FW", //old will be Forward
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get the total number of players for a match format (per team)
    /// </summary>
    public static int GetPlayersPerTeam(MatchFormat format)
    {
        return format switch
        {
            MatchFormat.FiveVsFive => 5,
            MatchFormat.SevenVsSeven => 7,
            MatchFormat.ElevenVsEleven => 11,
            _ => 0
        };
    }
}
