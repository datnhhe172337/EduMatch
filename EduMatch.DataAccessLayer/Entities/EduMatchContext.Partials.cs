using EduMatch.DataAccessLayer.Enum;
using Microsoft.EntityFrameworkCore;

namespace EduMatch.DataAccessLayer.Entities
{
    public partial class EduMatchContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // This code fixes the conflict by telling EF to store these enums as INTs.
            // This file will not be deleted when you re-scaffold.

            modelBuilder.Entity<WalletTransaction>(entity =>
            {
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.Reason).HasConversion<int>();
                entity.Property(e => e.TransactionType).HasConversion<int>();
            });

            modelBuilder.Entity<Deposit>(entity =>
            {
                entity.Property(e => e.Status).HasConversion<int>();
            });

            modelBuilder.Entity<Withdrawal>(entity =>
            {
                entity.Property(e => e.Status).HasConversion<int>();
            });

        }
    }
}