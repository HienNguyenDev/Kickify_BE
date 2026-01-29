using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Wallets.Queries.GetWalletTransactions;

public class GetWalletTransactionsQueryHandler : IQueryHandler<GetWalletTransactionsQuery, GetWalletTransactionsQueryResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPlayerWalletRepository _playerWalletRepository;
    private readonly IPlayerWalletTransactionRepository _playerWalletTransactionRepository;
    private readonly IVenueWalletRepository _venueWalletRepository;
    private readonly IVenueWalletTransactionRepository _venueWalletTransactionRepository;
    private readonly IUserContext _userContext;

    public GetWalletTransactionsQueryHandler(
        IUserRepository userRepository,
        IPlayerWalletRepository playerWalletRepository,
        IPlayerWalletTransactionRepository playerWalletTransactionRepository,
        IVenueWalletRepository venueWalletRepository,
        IVenueWalletTransactionRepository venueWalletTransactionRepository,
        IUserContext userContext)
    {
        _userRepository = userRepository;
        _playerWalletRepository = playerWalletRepository;
        _playerWalletTransactionRepository = playerWalletTransactionRepository;
        _venueWalletRepository = venueWalletRepository;
        _venueWalletTransactionRepository = venueWalletTransactionRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetWalletTransactionsQueryResponse>> Handle(GetWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.UserId);
        if (user is null)
        {
            return Result.Failure<GetWalletTransactionsQueryResponse>(UserErrors.NotFound(_userContext.UserId));
        }

        if (user.Role == UserRole.Player)
        {
            return await GetPlayerWalletTransactions(user.UserId, request, cancellationToken);
        }
        else if (user.Role == UserRole.VenueOwner)
        {
            return await GetVenueWalletTransactions(user.UserId, request, cancellationToken);
        }

        return Result.Failure<GetWalletTransactionsQueryResponse>(WalletErrors.InvalidRole);
    }

    private async Task<Result<GetWalletTransactionsQueryResponse>> GetPlayerWalletTransactions(
        Guid userId,
        GetWalletTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var wallet = await _playerWalletRepository.GetByUserIdAsync(userId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure<GetWalletTransactionsQueryResponse>(WalletErrors.WalletNotFound);
        }

        var (transactions, total) = await _playerWalletTransactionRepository.GetByWalletIdAsync(
            wallet.PlayerWalletId,
            request.TransactionType,
            request.Page,
            request.PageSize,
            cancellationToken);

        var transactionDtos = transactions.Select(t => new WalletTransactionDto
        {
            TransactionId = t.TransactionId,
            TransactionType = t.TransactionType.ToString(),
            Amount = t.Amount,
            BalanceAfter = t.BalanceAfter,
            TransactionCode = t.TransactionCode,
            ReferenceId = t.ReferenceId,
            Description = t.Description,
            CreatedAt = t.CreatedAt
        }).ToList();

        return Result.Success(new GetWalletTransactionsQueryResponse
        {
            WalletId = wallet.PlayerWalletId,
            WalletType = "Player",
            Transactions = transactionDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        });
    }

    private async Task<Result<GetWalletTransactionsQueryResponse>> GetVenueWalletTransactions(
        Guid userId,
        GetWalletTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var wallet = await _venueWalletRepository.GetByOwnerIdAsync(userId, cancellationToken);
        if (wallet is null)
        {
            return Result.Failure<GetWalletTransactionsQueryResponse>(WalletErrors.WalletNotFound);
        }

        var (transactions, total) = await _venueWalletTransactionRepository.GetByWalletIdAsync(
            wallet.VenueWalletId,
            request.TransactionType,
            request.Page,
            request.PageSize,
            cancellationToken);

        var transactionDtos = transactions.Select(t => new WalletTransactionDto
        {
            TransactionId = t.TransactionId,
            TransactionType = t.TransactionType.ToString(),
            Amount = t.Amount,
            BalanceAfter = t.BalanceAfter,
            TransactionCode = t.TransactionCode,
            ReferenceId = t.ReferenceId,
            Description = t.Description,
            CreatedAt = t.CreatedAt
        }).ToList();

        return Result.Success(new GetWalletTransactionsQueryResponse
        {
            WalletId = wallet.VenueWalletId,
            WalletType = "Venue",
            Transactions = transactionDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        });
    }
}
