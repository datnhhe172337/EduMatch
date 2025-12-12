using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.Wallet;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduMatch.UnitTests.WalletTests;

public sealed class WithdrawalServiceTests : IAsyncLifetime
{
    private readonly EduMatchContext _context;
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IWalletRepository> _walletRepository = new();
    private readonly Mock<IWithdrawalRepository> _withdrawalRepository = new();
    private readonly Mock<IWalletTransactionRepository> _walletTransactionRepository = new();
    private readonly Mock<IUserBankAccountRepository> _userBankAccountRepository = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly IMapper _mapper;
    private readonly WithdrawalService _sut;

    public WithdrawalServiceTests()
    {
        _context = WalletTestDbContextFactory.Create();

        _unitOfWork.SetupGet(u => u.Wallets).Returns(_walletRepository.Object);
        _unitOfWork.SetupGet(u => u.Withdrawals).Returns(_withdrawalRepository.Object);
        _unitOfWork.SetupGet(u => u.WalletTransactions).Returns(_walletTransactionRepository.Object);
        _unitOfWork.SetupGet(u => u.UserBankAccounts).Returns(_userBankAccountRepository.Object);
        _unitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<WithdrawalProfile>();
            cfg.AddProfile<UserBankAccountProfile>();
            cfg.AddProfile<WalletProfile>();
            cfg.AddProfile<BankProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _sut = new WithdrawalService(_unitOfWork.Object, _context, _mapper, _notificationService.Object);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    #region Query Tests

    [Fact]
    public async Task GetWithdrawalHistoryAsync_ReturnsMappedDtos()
    {
        const string userEmail = "history@test.com";
        var withdrawals = new List<Withdrawal>
        {
            new()
            {
                Id = 1,
                Amount = 80_000m,
                Status = WithdrawalStatus.Completed,
                UserBankAccount = new UserBankAccount
                {
                    AccountNumber = "123",
                    AccountHolderName = "User",
                    Bank = new Bank { Id = 1, Name = "Test Bank" }
                }
            }
        };

        _withdrawalRepository.Setup(r => r.GetWithdrawalsByUserEmailAsync(userEmail)).ReturnsAsync(withdrawals);

        var result = (await _sut.GetWithdrawalHistoryAsync(userEmail)).ToList();

        result.Should().HaveCount(1);
        result[0].Amount.Should().Be(20_000m);
        _withdrawalRepository.Verify(r => r.GetWithdrawalsByUserEmailAsync(userEmail), Times.Once);
    }

    [Fact]
    public async Task GetPendingWithdrawalsAsync_ReturnsAdminDtos()
    {
        var withdrawals = new List<Withdrawal>
        {
            new()
            {
                Id = 4,
                Amount = 80_000m,
                Status = WithdrawalStatus.Pending,
                UserBankAccount = new UserBankAccount
                {
                    AccountNumber = "456",
                    AccountHolderName = "Admin",
                    Bank = new Bank { Id = 2, Name = "Another" }
                },
                Wallet = new Wallet { UserEmail = "admin@test.com" }
            }
        };
        _withdrawalRepository.Setup(r => r.GetPendingWithdrawalsAsync()).ReturnsAsync(withdrawals);

        var result = (await _sut.GetPendingWithdrawalsAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Amount.Should().Be(30_000m);
        _withdrawalRepository.Verify(r => r.GetPendingWithdrawalsAsync(), Times.Once);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task CreateWithdrawalRequestAsync_ValidInput_ProcessesImmediatelyAndNotifies()
    {
        const string userEmail = "student@test.com";
        var wallet = new Wallet { Id = 1, UserEmail = userEmail, Balance = 200_000m, LockedBalance = 0 };
        var bankAccount = new UserBankAccount { Id = 15, UserEmail = userEmail };

        _userBankAccountRepository.Setup(r => r.GetByIdAsync(bankAccount.Id)).ReturnsAsync(bankAccount);
        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(userEmail)).ReturnsAsync(wallet);
        _withdrawalRepository.Setup(r => r.AddAsync(It.IsAny<Withdrawal>())).Returns(Task.CompletedTask);
        _walletTransactionRepository.Setup(r => r.AddAsync(It.IsAny<WalletTransaction>())).Returns(Task.CompletedTask);
        _unitOfWork.SetupSequence(u => u.CompleteAsync()).ReturnsAsync(1).ReturnsAsync(1);

        var request = new CreateWithdrawalRequest { Amount = 50_000m, UserBankAccountId = bankAccount.Id };

        await _sut.CreateWithdrawalRequestAsync(request, userEmail);

        wallet.Balance.Should().Be(150_000m);
        wallet.LockedBalance.Should().Be(0m);
        _withdrawalRepository.Verify(r => r.AddAsync(It.Is<Withdrawal>(w =>
            w.Amount == 50_000m &&
            w.Status == WithdrawalStatus.Completed &&
            w.UserBankAccountId == bankAccount.Id)), Times.Once);
        _notificationService.Verify(n => n.CreateNotificationAsync(
            userEmail,
            It.Is<string>(msg => msg.Contains("Yêu cầu rút tiền")),
            "/wallet/withdrawals"), Times.Once);
    }

    [Fact]
    public async Task CreateWithdrawalRequestAsync_MinAmountEqualBalance_LeavesZeroBalance()
    {
        const string userEmail = "student@test.com";
        var wallet = new Wallet { Id = 2, UserEmail = userEmail, Balance = 80_000m, LockedBalance = 0 };
        var bankAccount = new UserBankAccount { Id = 5, UserEmail = userEmail };

        _userBankAccountRepository.Setup(r => r.GetByIdAsync(bankAccount.Id)).ReturnsAsync(bankAccount);
        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(userEmail)).ReturnsAsync(wallet);
        _withdrawalRepository.Setup(r => r.AddAsync(It.IsAny<Withdrawal>())).Returns(Task.CompletedTask);
        _walletTransactionRepository.Setup(r => r.AddAsync(It.IsAny<WalletTransaction>())).Returns(Task.CompletedTask);
        _unitOfWork.SetupSequence(u => u.CompleteAsync()).ReturnsAsync(1).ReturnsAsync(1);

        var request = new CreateWithdrawalRequest { Amount = 80_000m, UserBankAccountId = bankAccount.Id };

        await _sut.CreateWithdrawalRequestAsync(request, userEmail);

        wallet.Balance.Should().Be(0m);
        _withdrawalRepository.Verify(r => r.AddAsync(It.Is<Withdrawal>(w => w.Amount == request.Amount)), Times.Once);
    }

    [Fact]
    public async Task CreateWithdrawalRequestAsync_InvalidBankAccount_Throws()
    {
        const string userEmail = "student@test.com";
        var request = new CreateWithdrawalRequest { Amount = 60_000m, UserBankAccountId = 999 };

        _userBankAccountRepository.Setup(r => r.GetByIdAsync(request.UserBankAccountId)).ReturnsAsync((UserBankAccount?)null);

        await _sut.Invoking(s => s.CreateWithdrawalRequestAsync(request, userEmail))
            .Should().ThrowAsync<System.Exception>()
            .WithMessage("Invalid bank account.*");
    }

    [Fact]
    public async Task CreateWithdrawalRequestAsync_BankAccountOwnedByDifferentUser_Throws()
    {
        const string userEmail = "student@test.com";
        var bankAccount = new UserBankAccount { Id = 11, UserEmail = "other@test.com" };
        _userBankAccountRepository.Setup(r => r.GetByIdAsync(bankAccount.Id)).ReturnsAsync(bankAccount);

        var request = new CreateWithdrawalRequest { Amount = 60_000m, UserBankAccountId = bankAccount.Id };

        await _sut.Invoking(s => s.CreateWithdrawalRequestAsync(request, userEmail))
            .Should().ThrowAsync<System.Exception>()
            .WithMessage("Invalid bank account.");
    }

    [Fact]
    public async Task CreateWithdrawalRequestAsync_WalletNotFound_Throws()
    {
        const string userEmail = "student@test.com";
        var bankAccount = new UserBankAccount { Id = 3, UserEmail = userEmail };
        _userBankAccountRepository.Setup(r => r.GetByIdAsync(bankAccount.Id)).ReturnsAsync(bankAccount);
        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(userEmail)).ReturnsAsync((Wallet?)null);

        var request = new CreateWithdrawalRequest { Amount = 60_000m, UserBankAccountId = bankAccount.Id };

        await _sut.Invoking(s => s.CreateWithdrawalRequestAsync(request, userEmail))
            .Should().ThrowAsync<System.Exception>()
            .WithMessage("Wallet not found.");
    }

    [Fact]
    public async Task CreateWithdrawalRequestAsync_InsufficientBalance_ThrowsAndDoesNotNotify()
    {
        const string userEmail = "student@test.com";
        var bankAccount = new UserBankAccount { Id = 1, UserEmail = userEmail };
        var wallet = new Wallet { Id = 5, UserEmail = userEmail, Balance = 30_000m };

        _userBankAccountRepository.Setup(r => r.GetByIdAsync(bankAccount.Id)).ReturnsAsync(bankAccount);
        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(userEmail)).ReturnsAsync(wallet);

        var request = new CreateWithdrawalRequest { Amount = 80_000m, UserBankAccountId = bankAccount.Id };

        await _sut.Invoking(s => s.CreateWithdrawalRequestAsync(request, userEmail))
            .Should().ThrowAsync<System.Exception>()
            .WithMessage("Không đủ số dư.");

        _notificationService.Verify(n => n.CreateNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateWithdrawalRequestAsync_FirstCommitFails_DoesNotChangeBalance()
    {
        const string userEmail = "student@test.com";
        var bankAccount = new UserBankAccount { Id = 21, UserEmail = userEmail };
        var wallet = new Wallet { Id = 9, UserEmail = userEmail, Balance = 100_000m };

        _userBankAccountRepository.Setup(r => r.GetByIdAsync(bankAccount.Id)).ReturnsAsync(bankAccount);
        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(userEmail)).ReturnsAsync(wallet);
        _withdrawalRepository.Setup(r => r.AddAsync(It.IsAny<Withdrawal>())).Returns(Task.CompletedTask);
        _unitOfWork.SetupSequence(u => u.CompleteAsync())
            .ThrowsAsync(new System.Exception("Commit failed"))
            .ReturnsAsync(1);

        var request = new CreateWithdrawalRequest { Amount = 80_000m, UserBankAccountId = bankAccount.Id };

        await _sut.Invoking(s => s.CreateWithdrawalRequestAsync(request, userEmail))
            .Should().ThrowAsync<System.Exception>()
            .WithMessage("Commit failed");

        wallet.Balance.Should().Be(100_000m);
        _walletTransactionRepository.Verify(r => r.AddAsync(It.IsAny<WalletTransaction>()), Times.Never);
        _notificationService.Verify(n => n.CreateNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    #endregion
}

