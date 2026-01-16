using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginCommandResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;
        private readonly IUnitOfWork _unitOfWork;

        public LoginCommandHandler(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IPasswordHasher passwordHasher, IJwtProvider jwtProvider, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<LoginCommandResponse>> Handle(LoginCommand request,CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user is null)
            {
                return Result.Failure<LoginCommandResponse>(UserErrors.NotFoundByEmail);
            }

            bool isValidPassword = _passwordHasher.Verify(request.Password, user.PasswordHash ?? "0");

            if (!isValidPassword)
            {
                return Result.Failure<LoginCommandResponse>(UserErrors.WrongPassword);
            }
            if (!user.IsActive)
            {
                return Result.Failure<LoginCommandResponse>(UserErrors.InActive);
            }

            if (!user.IsEmailVerified)
            {
                return Result.Failure<LoginCommandResponse>(UserErrors.IsNotVerified);
            }

            var token = await _jwtProvider.GetForCredentialsAsync(request.Email);
            RefreshToken refreshToken = new RefreshToken
            {
                TokenId = Guid.NewGuid(),
                Token = _jwtProvider.GenerateRefreshToken(),
                UserId = user.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            };

            var oldTokens = await _refreshTokenRepository.GetByUserIdAsync(user.UserId, cancellationToken);
            _refreshTokenRepository.RemoveRange(oldTokens);
            await _refreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new LoginCommandResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                IsEmailVerified = user.IsEmailVerified,
                IsActive = user.IsActive,
                AccessToken = token,
                RefreshToken = refreshToken.Token,
            };

            return Result.Success(response);
        }
    }
}
