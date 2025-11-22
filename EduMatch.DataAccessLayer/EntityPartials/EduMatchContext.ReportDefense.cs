using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduMatch.DataAccessLayer.Entities;

public partial class EduMatchContext
{
    private void ConfigureReportDefense(EntityTypeBuilder<ReportDefense> entity)
    {
        entity.ToTable("report_defenses");

        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.ReportId).HasColumnName("reportId");
        entity.Property(e => e.TutorEmail)
            .HasMaxLength(100)
            .HasColumnName("tutorEmail");
        entity.Property(e => e.Note)
            .HasMaxLength(2000)
            .HasColumnName("note");
        entity.Property(e => e.CreatedAt)
            .HasColumnType("datetime")
            .HasColumnName("createdAt");

        entity.HasOne(d => d.Report)
            .WithMany(p => p.ReportDefenses)
            .HasForeignKey(d => d.ReportId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_report_defenses_reports");
    }
}
