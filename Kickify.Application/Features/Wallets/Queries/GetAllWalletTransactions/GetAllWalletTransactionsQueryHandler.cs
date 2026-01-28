using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Wallets.Queries.GetAllWalletTransactions;

public class GetAllWalletTransactionsQueryHandler : IQueryHandler<GetAllWalletTransactionsQuery, GetAllWalletTransactionsQueryResponse>
{
    private readonly IPlayerWalletTransactionRepository _playerWalletTransactionRepository;
    private readonly IVenueWalletTransactionRepository _venueWalletTransactionRepository;

    public GetAllWalletTransactionsQueryHandler(
        IPlayerWalletTransactionRepository playerWalletTransactionRepository,
        IVenueWalletTransactionRepository venueWalletTransactionRepository)
    {
        _playerWalletTransactionRepository = playerWalletTransactionRepository;
        _venueWalletTransactionRepository = venueWalletTransactionRepository;
    }

    public async Task<Result<GetAllWalletTransactionsQueryResponse>> Handle(GetAllWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var allTransactions = new List<WalletTransactionItemDto>();
        int totalCount = 0;

        // Get Player Wallet Transactions
        if (request.WalletType == null || request.WalletType == WalletType.Player)
        {
            var (playerTransactions, playerTotal) = await _playerWalletTransactionRepository.GetAllAsync(
                request.TransactionType,
                request.Page,
                request.PageSize,
                cancellationToken);

            var playerDtos = playerTransactions.Select(t => new WalletTransactionItemDto
            {
                TransactionId = t.TransactionId,
                WalletId = t.PlayerWalletId,
                WalletType = "Player",
                UserId = t.PlayerWallet?.UserId ?? Guid.Empty,
                UserFullName = t.PlayerWallet?.User?.FullName,
                TransactionType = t.TransactionType.ToString(),
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                TransactionCode = t.TransactionCode,
                ReferenceId = t.ReferenceId,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            });

            if (request.WalletType == WalletType.Player)
            {
                return Result.Success(new GetAllWalletTransactionsQueryResponse
                {
                    Transactions = playerDtos.ToList(),
                    TotalCount = playerTotal,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(playerTotal / (double)request.PageSize)
                });
            }

            allTransactions.AddRange(playerDtos);
            totalCount += playerTotal;
        }

        // Get Venue Wallet Transactions
        if (request.WalletType == null || request.WalletType == WalletType.Venue)
        {
            var (venueTransactions, venueTotal) = await _venueWalletTransactionRepository.GetAllAsync(
                request.TransactionType,
                request.Page,
                request.PageSize,
                cancellationToken);

            var venueDtos = venueTransactions.Select(t => new WalletTransactionItemDto
            {
                TransactionId = t.TransactionId,
                WalletId = t.VenueWalletId,
                WalletType = "Venue",
                UserId = t.VenueWallet?.Venue?.OwnerId ?? Guid.Empty,
                UserFullName = t.VenueWallet?.Venue?.VenueName,
                TransactionType = t.TransactionType.ToString(),
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                TransactionCode = t.TransactionCode,
                ReferenceId = t.ReferenceId,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            });

            if (request.WalletType == WalletType.Venue)
            {
                return Result.Success(new GetAllWalletTransactionsQueryResponse
                {
                    Transactions = venueDtos.ToList(),
                    TotalCount = venueTotal,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(venueTotal / (double)request.PageSize)
                });
            }

            allTransactions.AddRange(venueDtos);
            totalCount += venueTotal;
        }

        // Sort all transactions by CreatedAt
        var sortedTransactions = allTransactions
            .OrderByDescending(t => t.CreatedAt)
            .ToList();

        return Result.Success(new GetAllWalletTransactionsQueryResponse
        {
            Transactions = sortedTransactions,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        });
    }
}
