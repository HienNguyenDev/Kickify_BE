using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Wallets.Queries.GetWalletBalance;

public class GetWalletBalanceQueryHandler : IQueryHandler<GetWalletBalanceQuery, GetWalletBalanceQueryResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPlayerWalletRepository _playerWalletRepository;
    private readonly IVenueWalletRepository _venueWalletRepository;
    private readonly IUserContext _userContext;

    public GetWalletBalanceQueryHandler(
        IUserRepository userRepository,
        IPlayerWalletRepository playerWalletRepository,
        IVenueWalletRepository venueWalletRepository,
        IUserContext userContext)
    {
        _userRepository = userRepository;
        _playerWalletRepository = playerWalletRepository;
        _venueWalletRepository = venueWalletRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetWalletBalanceQueryResponse>> Handle(GetWalletBalanceQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.UserId);
        if (user is null)
        {
            return Result.Failure<GetWalletBalanceQueryResponse>(UserErrors.NotFound(_userContext.UserId));
        }

        if (user.Role == UserRole.Player)
        {
            var wallet = await _playerWalletRepository.GetByUserIdAsync(user.UserId, cancellationToken);
            if (wallet is null)
            {
                return Result.Failure<GetWalletBalanceQueryResponse>(WalletErrors.WalletNotFound);
            }

            return Result.Success(new GetWalletBalanceQueryResponse
            {
                WalletId = wallet.PlayerWalletId,
                UserId = wallet.UserId,
                UserRole = UserRole.Player.ToString(),
                Balance = wallet.Balance,
                BankAccountNumber = wallet.BankAccountNumber,
                BankName = wallet.BankName,
                AccountHolderName = wallet.AccountHolderName
            });
        }
        else if (user.Role == UserRole.VenueOwner)
        {
            var wallet = await _venueWalletRepository.GetByOwnerIdAsync(user.UserId, cancellationToken);
            if (wallet is null)
            {
                return Result.Failure<GetWalletBalanceQueryResponse>(WalletErrors.WalletNotFound);
            }

            return Result.Success(new GetWalletBalanceQueryResponse
            {
                WalletId = wallet.VenueWalletId,
                UserId = user.UserId,
                UserRole = UserRole.VenueOwner.ToString(),
                Balance = wallet.Balance,
                BankAccountNumber = wallet.BankAccountNumber,
                BankName = wallet.BankName,
                AccountHolderName = wallet.AccountHolderName
            });
        }

        return Result.Failure<GetWalletBalanceQueryResponse>(WalletErrors.InvalidRole);
    }
}
