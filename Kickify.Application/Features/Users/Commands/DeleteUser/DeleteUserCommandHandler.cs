using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, DeleteUserCommandResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DeleteUserCommandResponse>> Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken)
        {
            // Get user by id
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user is null)
            {
                return Result.Failure<DeleteUserCommandResponse>(UserErrors.NotFound(request.UserId));
            }

            // Soft delete
            user.DeletedAt = DateTime.UtcNow;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new DeleteUserCommandResponse
            {
                UserId = user.UserId,
                DeletedAt = user.DeletedAt.Value
            };

            return Result.Success(response);
        }
    }
}
