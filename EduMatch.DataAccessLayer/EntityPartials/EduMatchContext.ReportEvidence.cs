using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduMatch.DataAccessLayer.Entities;

public partial class EduMatchContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportEvidence>(ConfigureReportEvidence);
        modelBuilder.Entity<ReportDefense>(ConfigureReportDefense);
    }

    private void ConfigureReportEvidence(EntityTypeBuilder<ReportEvidence> entity)
    {
        entity.ToTable("report_evidences");

        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.ReportId).HasColumnName("reportId");
        entity.Property(e => e.SubmittedByEmail)
            .HasMaxLength(100)
            .HasColumnName("submittedByEmail");
        entity.Property(e => e.MediaType).HasColumnName("mediaType");
        entity.Property(e => e.EvidenceType).HasColumnName("evidenceType");
        entity.Property(e => e.DefenseId).HasColumnName("defenseId");
        entity.Property(e => e.FileUrl)
            .HasMaxLength(500)
            .HasColumnName("fileUrl");
        entity.Property(e => e.FilePublicId)
            .HasMaxLength(255)
            .HasColumnName("filePublicId");
        entity.Property(e => e.Caption)
            .HasMaxLength(255)
            .HasColumnName("caption");
        entity.Property(e => e.CreatedAt)
            .HasColumnType("datetime")
            .HasColumnName("createdAt");

        entity.HasOne(d => d.Report)
            .WithMany(p => p.ReportEvidences)
            .HasForeignKey(d => d.ReportId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_report_evidences_reports");

        entity.HasOne(d => d.Defense)
            .WithMany(p => p.ReportEvidences)
            .HasForeignKey(d => d.DefenseId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_report_evidences_report_defenses");
    }
}
