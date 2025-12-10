using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduMatch.UnitTests.WalletTests;

public sealed class WalletServiceTests
{
    #region Setup
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IWalletRepository> _walletRepository = new();
    private readonly Mock<IWalletTransactionRepository> _transactionRepository = new();
    private readonly IMapper _mapper;
    private readonly WalletService _sut;

    public WalletServiceTests()
    {
        _unitOfWork.SetupGet(u => u.Wallets).Returns(_walletRepository.Object);
        _unitOfWork.SetupGet(u => u.WalletTransactions).Returns(_transactionRepository.Object);
        _unitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<WalletProfile>();
            cfg.AddProfile<WalletTransactionProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _sut = new WalletService(_unitOfWork.Object, _mapper);
    }
    #endregion

    #region GetOrCreateWalletForUserAsync Tests

    [Fact]
    public async Task GetOrCreateWalletForUserAsync_WalletExists_ReturnsDtoWithoutCreating()
    {
        var wallet = new Wallet { Id = 1, UserEmail = "existing@test.com", Balance = 20m, LockedBalance = 5m };
        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(wallet.UserEmail)).ReturnsAsync(wallet);

        var result = await _sut.GetOrCreateWalletForUserAsync(wallet.UserEmail);

        result.Should().NotBeNull();
        result!.Id.Should().Be(wallet.Id);
        result.Balance.Should().Be(wallet.Balance);
        _walletRepository.Verify(r => r.AddAsync(It.IsAny<Wallet>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateWalletForUserAsync_WalletMissing_CreatesAndReturnsDto()
    {
        const string userEmail = "new@test.com";
        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(userEmail)).ReturnsAsync((Wallet?)null);
        _walletRepository.Setup(r => r.AddAsync(It.IsAny<Wallet>())).Returns(Task.CompletedTask);

        var result = await _sut.GetOrCreateWalletForUserAsync(userEmail);

        result.Should().NotBeNull();
        result!.UserEmail.Should().Be(userEmail);
        _walletRepository.Verify(r => r.AddAsync(It.Is<Wallet>(w => w.UserEmail == userEmail)), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetOrCreateWalletForUserAsync_WalletExists_DoesNotPersist()
    {
        const string userEmail = "persist-check@test.com";
        var wallet = new Wallet { Id = 4, UserEmail = userEmail, Balance = 5m };
        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(userEmail)).ReturnsAsync(wallet);

        await _sut.GetOrCreateWalletForUserAsync(userEmail);

        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
        _walletRepository.Verify(r => r.AddAsync(It.IsAny<Wallet>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateWalletForUserAsync_NewWallet_InitializesBalances()
    {
        const string userEmail = "init@test.com";
        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(userEmail)).ReturnsAsync((Wallet?)null);
        _walletRepository.Setup(r => r.AddAsync(It.IsAny<Wallet>())).Returns(Task.CompletedTask);

        var result = await _sut.GetOrCreateWalletForUserAsync(userEmail);

        result.Should().NotBeNull();
        result!.Balance.Should().Be(0m);
        result.LockedBalance.Should().Be(0m);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
        _walletRepository.Verify(r => r.AddAsync(It.Is<Wallet>(w => w.UserEmail == userEmail)), Times.Once);
    }

    #endregion

    #region GetTransactionHistoryAsync Tests

    [Fact]
    public async Task GetTransactionHistoryAsync_WalletMissing_ReturnsEmptySequence()
    {
        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync("missing@test.com")).ReturnsAsync((Wallet?)null);

        var history = await _sut.GetTransactionHistoryAsync("missing@test.com");

        history.Should().BeEmpty();
        _transactionRepository.Verify(r => r.GetTransactionsByWalletIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_WalletExists_ReturnsMappedTransactions()
    {
        var wallet = new Wallet { Id = 2, UserEmail = "history@test.com" };
        var transactions = new List<WalletTransaction>
        {
            new()
            {
                Id = 1,
                WalletId = wallet.Id,
                Amount = 100m,
                TransactionType = WalletTransactionType.Credit,
                Reason = WalletTransactionReason.Deposit,
                Status = TransactionStatus.Completed
            },
            new()
            {
                Id = 2,
                WalletId = wallet.Id,
                Amount = 40m,
                TransactionType = WalletTransactionType.Debit,
                Reason = WalletTransactionReason.Withdrawal,
                Status = TransactionStatus.Pending
            }
        };

        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(wallet.UserEmail)).ReturnsAsync(wallet);
        _transactionRepository.Setup(r => r.GetTransactionsByWalletIdAsync(wallet.Id)).ReturnsAsync(transactions.AsEnumerable());

        var history = (await _sut.GetTransactionHistoryAsync(wallet.UserEmail)).ToList();

        history.Should().HaveCount(2);
        history.Select(h => h.Id).Should().BeEquivalentTo(new[] { 1, 2 });
        history.First().TransactionType.Should().Be(WalletTransactionType.Credit);
    }

    [Fact]
    public async Task GetTransactionHistoryAsync_WalletExists_NoTransactions_ReturnsEmpty()
    {
        var wallet = new Wallet { Id = 5, UserEmail = "empty@test.com" };
        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(wallet.UserEmail)).ReturnsAsync(wallet);
        _transactionRepository.Setup(r => r.GetTransactionsByWalletIdAsync(wallet.Id)).ReturnsAsync(Enumerable.Empty<WalletTransaction>());

        var history = await _sut.GetTransactionHistoryAsync(wallet.UserEmail);

        history.Should().BeEmpty();
    }
    #endregion
}
