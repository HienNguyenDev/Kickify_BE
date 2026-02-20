using FluentValidation;

namespace Kickify.Application.Features.NotificationPreferences.Commands.UpdateNotificationPreference;

public class UpdateNotificationPreferenceCommandValidator : AbstractValidator<UpdateNotificationPreferenceCommand>
{
    public UpdateNotificationPreferenceCommandValidator()
    {
        // All three properties are booleans with default values, no special validation needed.
        // Validator class is kept to follow the project convention.
    }
}
