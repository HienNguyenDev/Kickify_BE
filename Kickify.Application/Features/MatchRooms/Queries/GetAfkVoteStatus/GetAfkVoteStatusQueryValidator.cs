using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Queries.GetAfkVoteStatus;

public class GetAfkVoteStatusQueryValidator : AbstractValidator<GetAfkVoteStatusQuery>
{
    public GetAfkVoteStatusQueryValidator()
    {
        RuleFor(x => x.MatchRoomId).NotEmpty();
    }
}
