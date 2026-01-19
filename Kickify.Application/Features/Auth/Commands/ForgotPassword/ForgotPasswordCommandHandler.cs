using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, ForgotPasswordCommandResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IResetPasswordGenerator _resetPasswordGenerator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;

        public ForgotPasswordCommandHandler(IUserRepository userRepository, IResetPasswordGenerator resetPasswordGenerator, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher) 
        {
            _userRepository = userRepository;
            _resetPasswordGenerator = resetPasswordGenerator;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<ForgotPasswordCommandResponse>> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (user is null)
            {
                return Result.Failure<ForgotPasswordCommandResponse>(UserErrors.NotFoundByEmail);
            }

            var newPassword = _resetPasswordGenerator.GenerateRandomPassword();
            user.PasswordHash = _passwordHasher.Hash(newPassword);
            _userRepository.Update(user);
            user.Raise(new ForgotPasswordDomainEvent(user.Email, newPassword));
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new ForgotPasswordCommandResponse
            {
                Email = user.Email,
            };
            return Result.Success(response);
        }
    }
}
