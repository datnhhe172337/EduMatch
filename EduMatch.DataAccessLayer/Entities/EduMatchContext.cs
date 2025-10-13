using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EduMatch.DataAccessLayer.Entities;

public partial class EduMatchContext : DbContext
{
    public EduMatchContext()
    {
    }

    public EduMatchContext(DbContextOptions<EduMatchContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Level> Levels { get; set; }

    public virtual DbSet<Province> Provinces { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SubDistrict> SubDistricts { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<TimeSlot> TimeSlots { get; set; }

    public virtual DbSet<TutorAvailability> TutorAvailabilities { get; set; }

    public virtual DbSet<TutorCertificate> TutorCertificates { get; set; }

    public virtual DbSet<TutorEducation> TutorEducations { get; set; }

    public virtual DbSet<TutorProfile> TutorProfiles { get; set; }

    public virtual DbSet<TutorStatus> TutorStatuses { get; set; }

    public virtual DbSet<TutorSubject> TutorSubjects { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=72.60.209.239,1433; Database=EduMatch;UID=sa;PWD=FPTFall@2025!;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Level>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__levels__3213E83F447ADEA7");

            entity.ToTable("levels");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__province__3213E83FEF772C34");

            entity.ToTable("provinces");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__refresh___3213E83FEC9AF430");

            entity.ToTable("refresh_tokens");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.ExpiresAt)
                .HasPrecision(3)
                .HasColumnName("expiresAt");
            entity.Property(e => e.RevokedAt)
                .HasPrecision(3)
                .HasColumnName("revokedAt");
            entity.Property(e => e.TokenHash)
                .HasMaxLength(255)
                .HasColumnName("tokenHash");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("userEmail");

            entity.HasOne(d => d.UserEmailNavigation).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__refresh_t__userE__5BE2A6F2");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__roles__3213E83FA4CA3BF6");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "UQ__roles__B1947861AF18FA16").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("roleName");
        });

        modelBuilder.Entity<SubDistrict>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sub_dist__3213E83FE476F4FA");

            entity.ToTable("sub_districts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.ProvinceId).HasColumnName("provinceId");

            entity.HasOne(d => d.Province).WithMany(p => p.SubDistricts)
                .HasForeignKey(d => d.ProvinceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sub_distr__provi__619B8048");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__subjects__3213E83F62FBA01B");

            entity.ToTable("subjects");

            entity.HasIndex(e => e.SubjectName, "UQ__subjects__E5068BFD6270365C").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SubjectName)
                .HasMaxLength(200)
                .HasColumnName("subjectName");
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(e => e.SlotId).HasName("PK__time_slo__9C4A671372AA7FF6");

            entity.ToTable("time_slots");

            entity.Property(e => e.SlotId).HasColumnName("slotId");
            entity.Property(e => e.EndTime).HasColumnName("endTime");
            entity.Property(e => e.StartTime).HasColumnName("startTime");
        });

        modelBuilder.Entity<TutorAvailability>(entity =>
        {
            entity.HasKey(e => e.AvailabilityId).HasName("PK__tutor_av__BFEBC055485BDF39");

            entity.ToTable("tutor_availability");

            entity.Property(e => e.AvailabilityId).HasColumnName("availabilityId");
            entity.Property(e => e.DayOfWeek).HasColumnName("dayOfWeek");
            entity.Property(e => e.EffectiveFrom).HasColumnName("effectiveFrom");
            entity.Property(e => e.EffectiveTo).HasColumnName("effectiveTo");
            entity.Property(e => e.IsRecurring)
                .HasDefaultValue(true)
                .HasColumnName("isRecurring");
            entity.Property(e => e.SlotId).HasColumnName("slotId");
            entity.Property(e => e.TutorId).HasColumnName("tutorId");

            entity.HasOne(d => d.Slot).WithMany(p => p.TutorAvailabilities)
                .HasForeignKey(d => d.SlotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_ava__slotI__693CA210");

            entity.HasOne(d => d.Tutor).WithMany(p => p.TutorAvailabilities)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_ava__tutor__68487DD7");
        });

        modelBuilder.Entity<TutorCertificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_ce__3213E83F36FD8838");

            entity.ToTable("tutor_certificates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertificateUrl)
                .HasMaxLength(500)
                .HasColumnName("certificateUrl");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiryDate");
            entity.Property(e => e.IssueDate).HasColumnName("issueDate");
            entity.Property(e => e.Issuer)
                .HasMaxLength(200)
                .HasColumnName("issuer");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.TutorId).HasColumnName("tutorId");
            entity.Property(e => e.Verified).HasColumnName("verified");

            entity.HasOne(d => d.Tutor).WithMany(p => p.TutorCertificates)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_cer__tutor__5DCAEF64");
        });

        modelBuilder.Entity<TutorEducation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_ed__3213E83F221566E1");

            entity.ToTable("tutor_education");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertificateUrl)
                .HasMaxLength(500)
                .HasColumnName("certificateUrl");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.IssueDate).HasColumnName("issueDate");
            entity.Property(e => e.Issuer)
                .HasMaxLength(200)
                .HasColumnName("issuer");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.TutorId).HasColumnName("tutorId");
            entity.Property(e => e.Verified).HasColumnName("verified");

            entity.HasOne(d => d.Tutor).WithMany(p => p.TutorEducations)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_edu__tutor__6754599E");
        });

        modelBuilder.Entity<TutorProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_pr__3213E83F5EA8A0A2");

            entity.ToTable("tutor_profiles");

            entity.HasIndex(e => e.UserEmail, "UQ__tutor_pr__D54ADF55D0EACA98").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Gender)
                .HasMaxLength(20)
                .HasColumnName("gender");
            entity.Property(e => e.StatusId).HasColumnName("statusId");
            entity.Property(e => e.TeachingExp)
                .HasMaxLength(500)
                .HasColumnName("teachingExp");
            entity.Property(e => e.TeachingModes)
                .HasMaxLength(200)
                .HasColumnName("teachingModes");
            entity.Property(e => e.Title)
                .HasMaxLength(510)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasColumnName("updatedAt");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("userEmail");
            entity.Property(e => e.VideoIntroUrl)
                .HasMaxLength(500)
                .HasColumnName("videoIntroUrl");

            entity.HasOne(d => d.Status).WithMany(p => p.TutorProfiles)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_pro__statu__66603565");

            entity.HasOne(d => d.UserEmailNavigation).WithOne(p => p.TutorProfile)
                .HasForeignKey<TutorProfile>(d => d.UserEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_pro__userE__6477ECF3");
        });

        modelBuilder.Entity<TutorStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_st__3213E83FE21FE1B2");

            entity.ToTable("tutor_statuses");

            entity.HasIndex(e => e.StatusName, "UQ__tutor_st__6A50C212C66E9369").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("statusName");
        });

        modelBuilder.Entity<TutorSubject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_su__3213E83F6946E504");

            entity.ToTable("tutor_subjects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HourlyRate)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("hourlyRate");
            entity.Property(e => e.LevelId).HasColumnName("levelId");
            entity.Property(e => e.SubjectId).HasColumnName("subjectId");
            entity.Property(e => e.TutorId).HasColumnName("tutorId");

            entity.HasOne(d => d.Level).WithMany(p => p.TutorSubjects)
                .HasForeignKey(d => d.LevelId)
                .HasConstraintName("FK__tutor_sub__level__656C112C");

            entity.HasOne(d => d.Subject).WithMany(p => p.TutorSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_sub__subje__5FB337D6");

            entity.HasOne(d => d.Tutor).WithMany(p => p.TutorSubjects)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_sub__tutor__5EBF139D");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Email).HasName("PK__users__AB6E616593F17CA2");

            entity.ToTable("users");

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.IsEmailConfirmed).HasColumnName("isEmailConfirmed");
            entity.Property(e => e.LoginProvider)
                .HasMaxLength(50)
                .HasColumnName("loginProvider");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("passwordHash");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("roleId");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("userName");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__users__roleId__5CD6CB2B");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserEmail);

            entity.ToTable("user_profiles");

            entity.HasIndex(e => e.UserEmail, "UQ__user_pro__D54ADF55EDAB33DA").IsUnique();

            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("userEmail");
            entity.Property(e => e.AddressLine)
                .HasMaxLength(500)
                .HasColumnName("addressLine");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatarUrl");
            entity.Property(e => e.CityId).HasColumnName("cityId");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("longitude");
            entity.Property(e => e.SubDistrictId).HasColumnName("sub_district_id");

            entity.HasOne(d => d.City).WithMany(p => p.UserProfiles)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK__user_prof__cityI__60A75C0F");

            entity.HasOne(d => d.SubDistrict).WithMany(p => p.UserProfiles)
                .HasForeignKey(d => d.SubDistrictId)
                .HasConstraintName("FK__user_prof__sub_d__628FA481");

            entity.HasOne(d => d.UserEmailNavigation).WithOne(p => p.UserProfile)
                .HasForeignKey<UserProfile>(d => d.UserEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__user_prof__userE__6383C8BA");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
