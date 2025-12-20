using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Wallet;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduMatch.UnitTests.WalletTests;

public sealed class DepositServiceTests : IAsyncLifetime
{
    private readonly EduMatchContext _context;
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IDepositRepository> _depositRepository = new();
    private readonly Mock<IWalletRepository> _walletRepository = new();
    private readonly Mock<IWalletTransactionRepository> _walletTransactionRepository = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly Mock<EmailService> _emailService = new();
	private readonly IMapper _mapper = new Mock<IMapper>().Object;
    private readonly DepositService _sut;

    public DepositServiceTests()
    {
        _context = WalletTestDbContextFactory.Create();

        _unitOfWork.SetupGet(u => u.Deposits).Returns(_depositRepository.Object);
        _unitOfWork.SetupGet(u => u.Wallets).Returns(_walletRepository.Object);
        _unitOfWork.SetupGet(u => u.WalletTransactions).Returns(_walletTransactionRepository.Object);
        _unitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        _sut = new DepositService(_unitOfWork.Object, _mapper, _context, _notificationService.Object,_emailService.Object);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    #region CreateDepositRequestAsync Tests

    [Fact]
    public async Task CreateDepositRequestAsync_CreatesWalletWhenMissing()
    {
        const string userEmail = "new@test.com";
        var request = new WalletDepositRequest { Amount = 100_000 };
        Wallet? capturedWallet = null;

        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(userEmail)).ReturnsAsync((Wallet?)null);
        _walletRepository.Setup(r => r.AddAsync(It.IsAny<Wallet>()))
            .Callback<Wallet>(w => { w.Id = 10; capturedWallet = w; })
            .Returns(Task.CompletedTask);
        _depositRepository.Setup(r => r.AddAsync(It.IsAny<Deposit>())).Returns(Task.CompletedTask);

        var deposit = await _sut.CreateDepositRequestAsync(request, userEmail);

        capturedWallet.Should().NotBeNull();
        deposit.WalletId.Should().Be(10);
        deposit.Amount.Should().Be(request.Amount);
        _walletRepository.Verify(r => r.AddAsync(It.Is<Wallet>(w => w.UserEmail == userEmail)), Times.Once);
        _depositRepository.Verify(r => r.AddAsync(It.IsAny<Deposit>()), Times.Once);
    }

    [Fact]
    public async Task CreateDepositRequestAsync_UsesExistingWallet()
    {
        const string userEmail = "existing@test.com";
        var request = new WalletDepositRequest { Amount = 60_000 };
        var wallet = new Wallet { Id = 7, UserEmail = userEmail };

        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(userEmail)).ReturnsAsync(wallet);
        _depositRepository.Setup(r => r.AddAsync(It.IsAny<Deposit>())).Returns(Task.CompletedTask);

        var deposit = await _sut.CreateDepositRequestAsync(request, userEmail);

        deposit.WalletId.Should().Be(wallet.Id);
        _walletRepository.Verify(r => r.AddAsync(It.IsAny<Wallet>()), Times.Never);
    }

    [Fact]
    public async Task CreateDepositRequestAsync_WithMinimumAmount_AllowsAndReturnsDeposit()
    {
        const string userEmail = "zero@test.com";
        var request = new WalletDepositRequest { Amount = 50_000 };
        Wallet? captured = null;

        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(userEmail)).ReturnsAsync((Wallet?)null);
        _walletRepository.Setup(r => r.AddAsync(It.IsAny<Wallet>()))
            .Callback<Wallet>(w => captured = w)
            .Returns(Task.CompletedTask);
        _depositRepository.Setup(r => r.AddAsync(It.IsAny<Deposit>())).Returns(Task.CompletedTask);

        var deposit = await _sut.CreateDepositRequestAsync(request, userEmail);

        captured.Should().NotBeNull();
        deposit.Amount.Should().Be(50_000);
        deposit.WalletId.Should().Be(captured!.Id);
    }

    #endregion

    #region CleanupExpiredDepositsAsync Tests

    [Fact]
    public async Task CleanupExpiredDepositsAsync_MarksPendingOlderThan24Hours()
    {
        var expired = new List<Deposit>
        {
            new()
            {
                Id = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Status = TransactionStatus.Pending
            }
        };

        _depositRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Deposit, bool>>>()))
            .ReturnsAsync(expired);
        _unitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(expired.Count);

        var result = await _sut.CleanupExpiredDepositsAsync();

        result.Should().Be(expired.Count);
        expired[0].Status.Should().Be(TransactionStatus.Failed);
        _depositRepository.Verify(r => r.Update(expired[0]), Times.Once);
    }

    [Fact]
    public async Task CleanupExpiredDepositsAsync_NoExpiredDeposits_ReturnsZero()
    {
        _depositRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Deposit, bool>>>()))
            .ReturnsAsync(new List<Deposit>());

        var result = await _sut.CleanupExpiredDepositsAsync();

        result.Should().Be(0);
        _depositRepository.Verify(r => r.Update(It.IsAny<Deposit>()), Times.Never);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
    }

    #endregion

    #region CancelDepositRequestAsync Tests

    [Fact]
    public async Task CancelDepositRequestAsync_SucceedsForOwnerPending()
    {
        const string userEmail = "owner@test.com";
        var wallet = new Wallet { UserEmail = userEmail };
        var deposit = new Deposit { Id = 5, Wallet = wallet, Status = TransactionStatus.Pending };

        _depositRepository.Setup(r => r.GetByIdAsync(deposit.Id)).ReturnsAsync(deposit);
        _unitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var result = await _sut.CancelDepositRequestAsync(deposit.Id, userEmail);

        result.Should().BeTrue();
        deposit.Status.Should().Be(TransactionStatus.Failed);
        _depositRepository.Verify(r => r.Update(deposit), Times.Once);
    }

    [Fact]
    public async Task CancelDepositRequestAsync_ThrowsWhenNotOwner()
    {
        var wallet = new Wallet { UserEmail = "owner@test.com" };
        var deposit = new Deposit { Id = 5, Wallet = wallet, Status = TransactionStatus.Pending };

        _depositRepository.Setup(r => r.GetByIdAsync(deposit.Id)).ReturnsAsync(deposit);

        await _sut.Invoking(s => s.CancelDepositRequestAsync(deposit.Id, "intruder@test.com"))
            .Should().ThrowAsync<Exception>()
            .WithMessage("You do not have permission*");
    }

    [Fact]
    public async Task CancelDepositRequestAsync_ThrowsWhenNotPending()
    {
        var wallet = new Wallet { UserEmail = "owner@test.com" };
        var deposit = new Deposit { Id = 5, Wallet = wallet, Status = TransactionStatus.Completed };

        _depositRepository.Setup(r => r.GetByIdAsync(deposit.Id)).ReturnsAsync(deposit);

        await _sut.Invoking(s => s.CancelDepositRequestAsync(deposit.Id, wallet.UserEmail))
            .Should().ThrowAsync<Exception>()
            .WithMessage("This request cannot be cancelled*");
    }

    [Fact]
    public async Task CancelDepositRequestAsync_ThrowsWhenNotFound()
    {
        _depositRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Deposit?)null);

        await _sut.Invoking(s => s.CancelDepositRequestAsync(99, "user@test.com"))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Deposit request not found.");
    }

    #endregion

    #region ProcessVnpayPaymentAsync Tests

    [Fact]
    public async Task ProcessVnpayPaymentAsync_PendingDeposit_CreditsWalletAndNotifies()
    {
        var wallet = new Wallet { Id = 1, UserEmail = "learner@test.com", Balance = 100_000m };
        var deposit = new Deposit
        {
            Id = 9,
            WalletId = wallet.Id,
            Wallet = wallet,
            Amount = 50_000m,
            Status = TransactionStatus.Pending
        };

        _depositRepository.Setup(r => r.GetByIdAsync(deposit.Id)).ReturnsAsync(deposit);
        _walletTransactionRepository.Setup(r => r.AddAsync(It.IsAny<WalletTransaction>())).Returns(Task.CompletedTask);

        var result = await _sut.ProcessVnpayPaymentAsync(deposit.Id, "TXN-001", deposit.Amount);

        result.Should().BeTrue();
        deposit.Status.Should().Be(TransactionStatus.Completed);
        deposit.GatewayTransactionCode.Should().Be("TXN-001");
        wallet.Balance.Should().Be(150_000m);
        _walletTransactionRepository.Verify(r => r.AddAsync(It.Is<WalletTransaction>(t =>
            t.WalletId == wallet.Id &&
            t.Amount == deposit.Amount &&
            t.TransactionType == WalletTransactionType.Credit)), Times.Once);
        _notificationService.Verify(n =>
            n.CreateNotificationAsync(wallet.UserEmail,
                It.Is<string>(msg => msg.Contains("đã nạp thành công")),
                "/wallet/my-wallet"),
            Times.Once);
    }

    [Fact]
    public async Task ProcessVnpayPaymentAsync_DepositNotFound_Throws()
    {
        _depositRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Deposit?)null);

        await _sut.Invoking(s => s.ProcessVnpayPaymentAsync(999, "NA", 0m))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Deposit 999 not found.");
    }

    [Fact]
    public async Task ProcessVnpayPaymentAsync_AmountMismatch_ThrowsAndFailsDeposit()
    {
        var wallet = new Wallet { Id = 2, UserEmail = "user@test.com", Balance = 10m };
        var deposit = new Deposit
        {
            Id = 5,
            Wallet = wallet,
            WalletId = wallet.Id,
            Amount = 10m,
            Status = TransactionStatus.Pending
        };

        _depositRepository.Setup(r => r.GetByIdAsync(deposit.Id)).ReturnsAsync(deposit);

        var act = () => _sut.ProcessVnpayPaymentAsync(deposit.Id, "TXN-ERR", 7m);

        await act.Should().ThrowAsync<System.Exception>().WithMessage("*Amount mismatch*");
        deposit.Status.Should().Be(TransactionStatus.Failed);
        _notificationService.Verify(n => n.CreateNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _walletTransactionRepository.Verify(r => r.AddAsync(It.IsAny<WalletTransaction>()), Times.Never);
    }

    [Fact]
    public async Task ProcessVnpayPaymentAsync_DepositNotPending_ReturnsFalseWithoutUpdates()
    {
        var deposit = new Deposit { Id = 12, Status = TransactionStatus.Failed, Amount = 5m };
        _depositRepository.Setup(r => r.GetByIdAsync(deposit.Id)).ReturnsAsync(deposit);

        var result = await _sut.ProcessVnpayPaymentAsync(deposit.Id, "IGNORED", deposit.Amount);

        result.Should().BeFalse();
        _depositRepository.Verify(r => r.Update(It.IsAny<Deposit>()), Times.Never);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task ProcessVnpayPaymentAsync_PendingDepositWithoutWallet_ThrowsAndKeepsPending()
    {
        var deposit = new Deposit
        {
            Id = 15,
            WalletId = 77,
            Status = TransactionStatus.Pending,
            Amount = 25m,
            Wallet = null
        };
        _depositRepository.Setup(r => r.GetByIdAsync(deposit.Id)).ReturnsAsync(deposit);

        await _sut.Invoking(s => s.ProcessVnpayPaymentAsync(deposit.Id, "TXN-MISSING", deposit.Amount))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Wallet 77 not found.");

        deposit.Status.Should().Be(TransactionStatus.Pending);
        _walletTransactionRepository.Verify(r => r.AddAsync(It.IsAny<WalletTransaction>()), Times.Never);
    }

    [Fact]
    public async Task ProcessVnpayPaymentAsync_DepositAlreadyCompleted_ReturnsTrueWithoutChanges()
    {
        var wallet = new Wallet { Id = 3, Balance = 25m };
        var deposit = new Deposit
        {
            Id = 7,
            Wallet = wallet,
            WalletId = wallet.Id,
            Amount = 5m,
            Status = TransactionStatus.Completed
        };

        _depositRepository.Setup(r => r.GetByIdAsync(deposit.Id)).ReturnsAsync(deposit);

        var result = await _sut.ProcessVnpayPaymentAsync(deposit.Id, "IGNORED", deposit.Amount);

        result.Should().BeTrue();
        wallet.Balance.Should().Be(25m);
        _walletTransactionRepository.Verify(r => r.AddAsync(It.IsAny<WalletTransaction>()), Times.Never);
        _notificationService.Verify(n => n.CreateNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    #endregion
}
