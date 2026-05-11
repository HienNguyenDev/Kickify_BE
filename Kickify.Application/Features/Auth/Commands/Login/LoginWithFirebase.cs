using FirebaseAdmin.Auth;
using FluentValidation;
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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.Login
{
    public class LoginWithFirebase : ICommandHandler<LoginWithFirebaseCommand, LoginWithFirebaseCommandResponse>
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IWalletRepository _walletRepository;
    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly INotificationPreferenceRepository _notificationPreferenceRepository;
        private readonly IUnitOfWork _unitOfWork;
    public LoginWithFirebase(
        IJwtProvider jwtProvider,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IWalletRepository walletRepository,
        IPlayerProfileRepository playerProfileRepository,
        INotificationPreferenceRepository notificationPreferenceRepository,
        IUnitOfWork unitOfWork)
        {
            _jwtProvider = jwtProvider;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _walletRepository = walletRepository;
        _playerProfileRepository = playerProfileRepository;
        _notificationPreferenceRepository = notificationPreferenceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<LoginWithFirebaseCommandResponse>> Handle(LoginWithFirebaseCommand command, CancellationToken cancellationToken)
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
                    Role = UserRole.Player,
                    CreatedAt = DateTime.UtcNow,
                    IdentityId = identityId,
                    IsEmailVerified = true
                };
                await _userRepository.AddAsync(user);

                var wallet = new Wallet
                {
                    WalletId = Guid.NewGuid(),
                    UserId = user.UserId,
                    WalletType = WalletType.Player,
                    Balance = 0,
                };
                await _walletRepository.AddAsync(wallet);

                var playerProfile = new PlayerProfile
                {
                    ProfileId = Guid.NewGuid(),
                    UserId = user.UserId,
                    CurrentElo = 1000,
                    CurrentRank = "Amateur",
                    IsLegend = false,
                    TrustScore = 100,
                    TotalMatches = 0,
                    Wins = 0,
                    Losses = 0,
                    Draws = 0,
                    MvpCount = 0,
                    WinStreak = 0,
                    MaxWinStreak = 0,
                    ReportCount = 0,
                };
                await _playerProfileRepository.AddAsync(playerProfile);

                var notificationPreference = new NotificationPreference
                {
                    PreferenceId = Guid.NewGuid(),
                    UserId = user.UserId,
                    MatchRoom = true,
                    Friendship = true,
                    Post = true,
                    Chat = true
                };
                await _notificationPreferenceRepository.AddAsync(notificationPreference);
            }
            else
            {
                // Auto-unban nếu đã hết thời gian ban
                if (!user.IsActive && user.BannedUntil.HasValue && user.BannedUntil.Value <= DateTime.UtcNow)
                {
                    user.IsActive = true;
                    user.BannedUntil = null;
                    _userRepository.Update(user);
                }

                if (!user.IsActive)
                    return Result.Failure<LoginWithFirebaseCommandResponse>(UserErrors.InActive);
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

            var response = new LoginWithFirebaseCommandResponse
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
                IsPremium = user.IsPremium,
                PremiumExpireAt = user.PremiumExpireAt,
                CreatedAt = user.CreatedAt
            };
            return Result.Success(response);
        }
    }

    public class LoginWithFirebaseCommand() : ICommand<LoginWithFirebaseCommandResponse>
    {
        public string Uid { get; set; } = string.Empty;
    }

    public sealed class LoginWithFirebaseCommandResponse
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
        public bool IsPremium { get; set; }
        public DateTime? PremiumExpireAt { get; set; }
    }

    public class LoginWithFirebaseCommandValidator : AbstractValidator<LoginWithFirebaseCommand>
    {
        public LoginWithFirebaseCommandValidator()
        {
            RuleFor(x => x.Uid).NotEmpty();
        }
    }
}
