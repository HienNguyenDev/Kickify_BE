using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Kickify.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, ChangePasswordCommandResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public ChangePasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ChangePasswordCommandResponse>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null)
            {
                return Result.Failure<ChangePasswordCommandResponse>(UserErrors.NotFoundByEmail);
            }
            if (user.PasswordHash != request.CurrentPassword)
            {
                return Result.Failure<ChangePasswordCommandResponse>(UserErrors.WrongPassword);
            }
            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var response = new ChangePasswordCommandResponse
            {
                UserId = user.UserId
            };
            return Result.Success(response);
        }
    }
}
