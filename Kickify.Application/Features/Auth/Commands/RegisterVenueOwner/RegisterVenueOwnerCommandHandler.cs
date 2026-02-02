using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.OTP;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;

namespace Kickify.Application.Features.Auth.Commands.RegisterVenueOwner;

public class RegisterVenueOwnerCommandHandler : ICommandHandler<RegisterVenueOwnerCommand, RegisterVenueOwnerCommandResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IAuthenticationServices _authenticationServices;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IOtpGenerator _otpGenerator;
    private readonly IRedisOtpStore _otpStore;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterVenueOwnerCommandHandler(
        IUserRepository userRepository,
        IWalletRepository walletRepository,
        IAuthenticationServices authenticationServices,
        IPasswordHasher passwordHasher,
        IOtpGenerator otpGenerator,
        IRedisOtpStore otpStore,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _authenticationServices = authenticationServices;
        _passwordHasher = passwordHasher;
        _otpGenerator = otpGenerator;
        _otpStore = otpStore;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegisterVenueOwnerCommandResponse>> Handle(RegisterVenueOwnerCommand request, CancellationToken cancellationToken)
    {
        var userExist = await _userRepository.GetUserByEmailIgnoreFilterAsync(request.Email);
        if (userExist != null && userExist.DeletedAt == null)
        {
            return Result.Failure<RegisterVenueOwnerCommandResponse>(UserErrors.EmailAlreadyExists);
        }

        var identityId = await _authenticationServices.RegisterAsync(request.Email, request.Password);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.VenueOwner,
            IdentityId = identityId,
            IsActive = true,
            IsEmailVerified = false,
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

        var otp = _otpGenerator.Generate6Digits();
        await _otpStore.StoreAsync(user.UserId, otp, TimeSpan.FromMinutes(5), cancellationToken);
        user.Raise(new RegisterVenueOwnerDomainEvent(user.UserId, user.Email, otp));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RegisterVenueOwnerCommandResponse
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
