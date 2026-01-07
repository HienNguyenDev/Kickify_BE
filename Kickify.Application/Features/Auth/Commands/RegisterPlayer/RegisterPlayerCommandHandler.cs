using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.RegisterPlayer
{
    public class RegisterPlayerCommandHandler : ICommandHandler<RegisterPlayerCommand, RegisterPlayerCommandResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationServices _authenticationServices;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterPlayerCommandHandler(IUserRepository userRepository, IAuthenticationServices authenticationServices, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _authenticationServices = authenticationServices;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RegisterPlayerCommandResponse>> Handle(RegisterPlayerCommand request, CancellationToken cancellationToken)
        {
            var userExist = await _userRepository.IsEmailExistsAsync(request.Email);
            if (userExist)
            {
                return Result.Failure<RegisterPlayerCommandResponse>(UserErrors.EmailAlreadyExists);
            }

            var identityId = await _authenticationServices.RegisterAsync(request.Email, request.Password);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = UserRole.Player,
                IdentityId = identityId,
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };  

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new RegisterPlayerCommandResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                IdentityId = user.IdentityId,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt

            };
            return Result.Success(response);
        }
    }
}
