using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRoomById
{
    public class GetMatchRoomByIdQueryValidator : AbstractValidator<GetMatchRoomByIdQuery>
    {
        public GetMatchRoomByIdQueryValidator()
        {
            RuleFor(x => x.RoomId)
                .NotEmpty().WithMessage("Room ID is required");
        }
    }
}
