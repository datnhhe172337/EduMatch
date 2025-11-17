using System;
using EduMatch.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EduMatch.UnitTests.WalletTests;

/// <summary>
/// Creates an EduMatchContext backed by EF Core's in-memory provider so services that
/// open database transactions (Deposit/Withdrawal) can run without SQL Server.
/// </summary>
internal static class WalletTestDbContextFactory
{
    public static EduMatchContext Create()
    {
        var options = new DbContextOptionsBuilder<EduMatchContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new InMemoryEduMatchContext(options);
    }

    private sealed class InMemoryEduMatchContext : EduMatchContext
    {
        public InMemoryEduMatchContext(DbContextOptions<EduMatchContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            }
        }
    }
}
