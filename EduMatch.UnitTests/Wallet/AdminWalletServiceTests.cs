using System.Threading.Tasks;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduMatch.UnitTests.WalletTests;

public sealed class AdminWalletServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IWalletRepository> _walletRepository = new();
    private readonly AdminWalletService _sut;

    public AdminWalletServiceTests()
    {
        _unitOfWork.SetupGet(u => u.Wallets).Returns(_walletRepository.Object);
        _unitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        _sut = new AdminWalletService(_unitOfWork.Object);
    }

    #region GetSystemWalletDashboardAsync Tests

    [Fact]
    public async Task GetSystemWalletDashboardAsync_UsesExistingSystemWalletAndAggregates()
    {
        var systemWallet = new Wallet
        {
            Id = 1,
            UserEmail = "system@edumatch.com",
            Balance = 500m,
            LockedBalance = 200m
        };
        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync(systemWallet.UserEmail))
            .ReturnsAsync(systemWallet);
        _walletRepository.Setup(r => r.GetTotalLockedBalanceForRoleAsync("Tutor"))
            .ReturnsAsync(150m);
        _walletRepository.Setup(r => r.GetTotalAvailableBalanceAsync())
            .ReturnsAsync(1_000m);

        var dashboard = await _sut.GetSystemWalletDashboardAsync();

        dashboard.Should().BeEquivalentTo(new SystemWalletDashboardDto
        {
            PendingTutorPayoutBalance = 200m,
            PlatformRevenueBalance = 500m,
            TotalUserAvailableBalance = 1_000m
        });
        _walletRepository.Verify(r => r.AddAsync(It.IsAny<Wallet>()), Times.Never);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
        _walletRepository.Verify(r => r.GetTotalLockedBalanceForRoleAsync("Tutor"), Times.Once);
    }

    [Fact]
    public async Task GetSystemWalletDashboardAsync_SystemWalletMissing_CreatesAndReturnsZeroBalances()
    {
        Wallet? created = null;
        _walletRepository.Setup(r => r.GetWalletByUserEmailAsync("system@edumatch.com"))
            .ReturnsAsync((Wallet?)null);
        _walletRepository.Setup(r => r.AddAsync(It.IsAny<Wallet>()))
            .Callback<Wallet>(w => created = w)
            .Returns(Task.CompletedTask);
        _walletRepository.Setup(r => r.GetTotalLockedBalanceForRoleAsync("Tutor"))
            .ReturnsAsync(0m);
        _walletRepository.Setup(r => r.GetTotalAvailableBalanceAsync())
            .ReturnsAsync(500m);

        var dashboard = await _sut.GetSystemWalletDashboardAsync();

        created.Should().NotBeNull();
        created!.UserEmail.Should().Be("system@edumatch.com");
        created.Balance.Should().Be(0m);
        created.LockedBalance.Should().Be(0m);
        dashboard.PlatformRevenueBalance.Should().Be(0m);
        dashboard.PendingTutorPayoutBalance.Should().Be(0m);
        dashboard.TotalUserAvailableBalance.Should().Be(500m);
        _walletRepository.Verify(r => r.AddAsync(It.IsAny<Wallet>()), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }

    #endregion
}
