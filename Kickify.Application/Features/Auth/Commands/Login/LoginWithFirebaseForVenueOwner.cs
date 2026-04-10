using FirebaseAdmin.Auth;
using FluentValidation;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.Login
{
    public class LoginWithFirebaseForVenueOwner : ICommandHandler<LoginWithFirebaseForVenueOwnerCommand, LoginWithFirebaseForVenueOwnerCommandResponse>
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
    public LoginWithFirebaseForVenueOwner(
        IJwtProvider jwtProvider,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IWalletRepository walletRepository,
        IUnitOfWork unitOfWork)
        {
            _jwtProvider = jwtProvider;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<LoginWithFirebaseForVenueOwnerCommandResponse>> Handle(LoginWithFirebaseForVenueOwnerCommand command, CancellationToken cancellationToken)
        {
            var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(command.Uid, cancellationToken);

            string email = userRecord.Email;
            string name = userRecord.DisplayName;
            string identityId = userRecord.Uid;

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = email,
                    FullName = name,
                    Role = UserRole.VenueOwner,
                    CreatedAt = DateTime.UtcNow,
                    IdentityId = identityId,
                    IsEmailVerified = true
                };
                await _userRepository.AddAsync(user);

                var wallet = new Wallet
                {
                    WalletId = Guid.NewGuid(),
                    UserId = user.UserId,
                    WalletType = WalletType.VenueOwner,
                    Balance = 0,
                };
                await _walletRepository.AddAsync(wallet);
            }

            var accessToken = _jwtProvider.GenerateBackendJwt(user);
            RefreshToken refreshToken = new RefreshToken
            {
                TokenId = Guid.NewGuid(),
                Token = _jwtProvider.GenerateRefreshToken(),
                UserId = user.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            };

            if (user != null)
            {
                var oldTokens = await _refreshTokenRepository.GetByUserIdAsync(user.UserId, cancellationToken);
                _refreshTokenRepository.RemoveRange(oldTokens);
            }

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new LoginWithFirebaseForVenueOwnerCommandResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                AvatarUrl = user.AvatarUrl,
                IsEmailVerified = user.IsEmailVerified,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
            return Result.Success(response);
        }
    }

    public class LoginWithFirebaseForVenueOwnerCommand() : ICommand<LoginWithFirebaseForVenueOwnerCommandResponse>
    {
        public string Uid { get; set; } = string.Empty;
    }

    public sealed class LoginWithFirebaseForVenueOwnerCommandResponse
    {
        public Guid UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsActive { get; set; }
    }

    public class LoginWithFirebaseForVenueOwnerCommandValidator : AbstractValidator<LoginWithFirebaseForVenueOwnerCommand>
    {
        public LoginWithFirebaseForVenueOwnerCommandValidator()
        {
            RuleFor(x => x.Uid).NotEmpty();
        }
    }
}
