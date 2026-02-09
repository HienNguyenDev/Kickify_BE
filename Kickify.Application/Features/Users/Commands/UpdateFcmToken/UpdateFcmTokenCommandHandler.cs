using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Users.Commands.UpdateFcmToken;

public class UpdateFcmTokenCommandHandler : ICommandHandler<UpdateFcmTokenCommand, UpdateFcmTokenCommandResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFcmTokenCommandHandler(
        IUserRepository userRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateFcmTokenCommandResponse>> Handle(UpdateFcmTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.UserId);
        if (user == null)
        {
            return Result.Failure<UpdateFcmTokenCommandResponse>(UserErrors.NotFound(_userContext.UserId));
        }

        user.FcmToken = request.FcmToken;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateFcmTokenCommandResponse { Success = true });
    }
}
