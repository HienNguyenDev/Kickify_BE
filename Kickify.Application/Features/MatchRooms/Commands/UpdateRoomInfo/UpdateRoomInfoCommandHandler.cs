using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateRoomInfo
{
    public class UpdateRoomInfoCommandHandler : ICommandHandler<UpdateRoomInfoCommand, UpdateRoomInfoResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

        public UpdateRoomInfoCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IUnitOfWork unitOfWork,
            IUserContext userContext)
        {
            _matchRoomRepository = matchRoomRepository;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        public async Task<Result<UpdateRoomInfoResponse>> Handle(UpdateRoomInfoCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;

            var room = await _matchRoomRepository.GetByIdAsync(request.RoomId);
            if (room == null)
            {
                return Result.Failure<UpdateRoomInfoResponse>(MatchRoomErrors.NotFound(request.RoomId));
            }

            if (room.HostId != userId)
            {
                return Result.Failure<UpdateRoomInfoResponse>(MatchRoomErrors.OnlyHostCanUpdateRoomInfo);
            }

            if (request.RoomName != null)
            {
                room.RoomName = request.RoomName;
            }

            if (request.Description != null)
            {
                room.Description = request.Description;
            }

            if (request.Rules != null)
            {
                room.Rules = request.Rules;
            }

            room.UpdatedAt = DateTime.UtcNow;

            _matchRoomRepository.Update(room);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new UpdateRoomInfoResponse(
                room.RoomId,
                room.RoomName,
                room.Description,
                room.Rules,
                room.UpdatedAt
            ));
        }
    }
}
