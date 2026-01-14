using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.Login
{
    public class LoginWithRefreshToken : ICommandHandler<LoginWithRefreshTokenCommand, LoginWithRefreshTokenCommandResponse>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtProvider _jwtProvider;
        private readonly IUnitOfWork _unitOfWork;

        public LoginWithRefreshToken(IRefreshTokenRepository refreshTokenRepository, IJwtProvider jwtProvider, IUnitOfWork unitOfWork)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _jwtProvider = jwtProvider;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<LoginWithRefreshTokenCommandResponse>> Handle(LoginWithRefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenWithUserAsync(command.RefreshToken, cancellationToken);

            if (refreshToken == null)
            {
                return Result.Failure<LoginWithRefreshTokenCommandResponse>(UserErrors.InvalidRefreshToken);
            }

            if (refreshToken.RevokedAt != null)
            {
                await _refreshTokenRepository.RevokeAllUserTokensAsync(refreshToken.UserId, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Failure<LoginWithRefreshTokenCommandResponse>(UserErrors.TokenReuseDetected);
            }

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return Result.Failure<LoginWithRefreshTokenCommandResponse>(UserErrors.RefreshTokenExpired);
            }

            string accessToken = _jwtProvider.GenerateBackendJwt(refreshToken.User);
            string newRefreshTokenString = _jwtProvider.GenerateRefreshToken();

            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.ReplacedByToken = newRefreshTokenString;
            _refreshTokenRepository.Update(refreshToken);

            var newRefreshToken = new RefreshToken
            {
                Token = newRefreshTokenString,
                UserId = refreshToken.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            };
            await _refreshTokenRepository.AddAsync(newRefreshToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new LoginWithRefreshTokenCommandResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
            return Result.Success(response);
        }
    }
    public class LoginWithRefreshTokenCommand : ICommand<LoginWithRefreshTokenCommandResponse>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class LoginWithRefreshTokenCommandResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}