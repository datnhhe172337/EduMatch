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

    public virtual DbSet<CertificateType> CertificateTypes { get; set; }

    public virtual DbSet<CertificateTypeSubject> CertificateTypeSubjects { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatRoom> ChatRooms { get; set; }

    public virtual DbSet<EducationInstitution> EducationInstitutions { get; set; }

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

    public virtual DbSet<TutorSubject> TutorSubjects { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=72.60.209.239,1433;Database=EduMatch_v1;User ID=sa;Password=FPTFall@2025!;Encrypt=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CertificateType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__certific__3213E83FC9E555E2");

            entity.ToTable("certificate_types");

            entity.HasIndex(e => e.Code, "UQ__certific__357D4CF92A8BE488").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CertificateTypeSubject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__certific__3213E83FF192111C");

            entity.ToTable("certificate_type_subjects");

            entity.HasIndex(e => new { e.CertificateTypeId, e.SubjectId }, "certificate_type_subjects_index_0").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertificateTypeId).HasColumnName("certificateTypeId");
            entity.Property(e => e.SubjectId).HasColumnName("subjectId");

            entity.HasOne(d => d.CertificateType).WithMany(p => p.CertificateTypeSubjects)
                .HasForeignKey(d => d.CertificateTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__certifica__certi__6754599E");

            entity.HasOne(d => d.Subject).WithMany(p => p.CertificateTypeSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__certifica__subje__68487DD7");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__chat_mes__3213E83FE96FA10E");

            entity.ToTable("chat_messages");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChatRoomId).HasColumnName("chat_room_id");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.MessageText).HasColumnName("message_text");
            entity.Property(e => e.ReceiverEmail)
                .HasMaxLength(100)
                .HasColumnName("receiver_email");
            entity.Property(e => e.SenderEmail)
                .HasMaxLength(100)
                .HasColumnName("sender_email");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("sent_at");

            entity.HasOne(d => d.ChatRoom).WithMany(p => p.ChatMessages).HasForeignKey(d => d.ChatRoomId);
        });

        modelBuilder.Entity<ChatRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__chat_roo__3213E83F7E89E749");

            entity.ToTable("chat_room");

            entity.HasIndex(e => new { e.UserEmail, e.TutorId }, "UQ_chat_room_user_tutor").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.TutorId).HasColumnName("tutor_id");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("user_email");

            entity.HasOne(d => d.Tutor).WithMany(p => p.ChatRooms).HasForeignKey(d => d.TutorId);

            entity.HasOne(d => d.UserEmailNavigation).WithMany(p => p.ChatRooms).HasForeignKey(d => d.UserEmail);
        });

        modelBuilder.Entity<EducationInstitution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__educatio__3213E83FFF7331E8");

            entity.ToTable("education_institutions");

            entity.HasIndex(e => e.Code, "UQ__educatio__357D4CF90B73C71D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.InstitutionType).HasColumnName("institutionType");
            entity.Property(e => e.Name)
                .HasMaxLength(300)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Level>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__levels__3213E83F3A1C9A36");

            entity.ToTable("levels");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__province__3213E83F688ECBDC");

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
            entity.HasKey(e => e.Id).HasName("PK__refresh___3213E83F537F03B3");

            entity.ToTable("refresh_tokens");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.ExpiresAt).HasColumnName("expiresAt");
            entity.Property(e => e.RevokedAt).HasColumnName("revokedAt");
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
            entity.HasKey(e => e.Id).HasName("PK__roles__3213E83F28BEE802");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "UQ__roles__B1947861DCC923FD").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("roleName");
        });

        modelBuilder.Entity<SubDistrict>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sub_dist__3213E83F66534916");

            entity.ToTable("sub_district");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.ProvinceId).HasColumnName("provinceId");

            entity.HasOne(d => d.Province).WithMany(p => p.SubDistricts)
                .HasForeignKey(d => d.ProvinceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sub_distr__provi__60A75C0F");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__subjects__3213E83FAB8828FA");

            entity.ToTable("subjects");

            entity.HasIndex(e => e.SubjectName, "UQ__subjects__E5068BFD4F2F1D36").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SubjectName)
                .HasMaxLength(200)
                .HasColumnName("subjectName");
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__time_slo__3213E83F26A39A86");

            entity.ToTable("time_slots");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndTime).HasColumnName("endTime");
            entity.Property(e => e.StartTime).HasColumnName("startTime");
        });

        modelBuilder.Entity<TutorAvailability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_av__3213E83F6FEFE9BB");

            entity.ToTable("tutor_availability");

            entity.Property(e => e.Id).HasColumnName("id");
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
                .HasConstraintName("FK__tutor_ava__slotI__6C190EBB");

            entity.HasOne(d => d.Tutor).WithMany(p => p.TutorAvailabilities)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_ava__tutor__6B24EA82");
        });

        modelBuilder.Entity<TutorCertificate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_ce__3213E83F3FCACDB2");

            entity.ToTable("tutor_certificates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertificatePublicId)
                .HasMaxLength(200)
                .HasColumnName("certificatePublicId");
            entity.Property(e => e.CertificateTypeId).HasColumnName("certificateTypeId");
            entity.Property(e => e.CertificateUrl)
                .HasMaxLength(200)
                .HasColumnName("certificateUrl");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiryDate");
            entity.Property(e => e.IssueDate).HasColumnName("issueDate");
            entity.Property(e => e.RejectReason)
                .HasMaxLength(300)
                .HasColumnName("rejectReason");
            entity.Property(e => e.TutorId).HasColumnName("tutorId");
            entity.Property(e => e.Verified).HasColumnName("verified");

            entity.HasOne(d => d.CertificateType).WithMany(p => p.TutorCertificates)
                .HasForeignKey(d => d.CertificateTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_cer__certi__656C112C");

            entity.HasOne(d => d.Tutor).WithMany(p => p.TutorCertificates)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_cer__tutor__66603565");
        });

        modelBuilder.Entity<TutorEducation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_ed__3213E83F3A588FD9");

            entity.ToTable("tutor_education");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertificatePublicId)
                .HasMaxLength(200)
                .HasColumnName("certificatePublicId");
            entity.Property(e => e.CertificateUrl)
                .HasMaxLength(200)
                .HasColumnName("certificateUrl");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.InstitutionId).HasColumnName("institutionId");
            entity.Property(e => e.IssueDate).HasColumnName("issueDate");
            entity.Property(e => e.RejectReason)
                .HasMaxLength(300)
                .HasColumnName("rejectReason");
            entity.Property(e => e.TutorId).HasColumnName("tutorId");
            entity.Property(e => e.Verified).HasColumnName("verified");

            entity.HasOne(d => d.Institution).WithMany(p => p.TutorEducations)
                .HasForeignKey(d => d.InstitutionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_edu__insti__693CA210");

            entity.HasOne(d => d.Tutor).WithMany(p => p.TutorEducations)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_edu__tutor__6A30C649");
        });

        modelBuilder.Entity<TutorProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_pr__3213E83F924DBCEA");

            entity.ToTable("tutor_profiles");

            entity.HasIndex(e => e.UserEmail, "UQ__tutor_pr__D54ADF55BAF574EE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TeachingExp)
                .HasMaxLength(500)
                .HasColumnName("teachingExp");
            entity.Property(e => e.TeachingModes).HasColumnName("teachingModes");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("userEmail");
            entity.Property(e => e.VideoIntroPublicId)
                .HasMaxLength(200)
                .HasColumnName("videoIntroPublicId");
            entity.Property(e => e.VideoIntroUrl)
                .HasMaxLength(200)
                .HasColumnName("videoIntroUrl");

            entity.HasOne(d => d.UserEmailNavigation).WithOne(p => p.TutorProfile)
                .HasForeignKey<TutorProfile>(d => d.UserEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_pro__userE__6383C8BA");
        });

        modelBuilder.Entity<TutorSubject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_su__3213E83F05BD6105");

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
                .HasConstraintName("FK__tutor_sub__level__6477ECF3");

            entity.HasOne(d => d.Subject).WithMany(p => p.TutorSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_sub__subje__5EBF139D");

            entity.HasOne(d => d.Tutor).WithMany(p => p.TutorSubjects)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tutor_sub__tutor__5DCAEF64");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Email).HasName("PK__users__AB6E61654D12C769");

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
            entity.HasKey(e => e.UserEmail).HasName("PK__user_pro__D54ADF5463AD4278");

            entity.ToTable("user_profiles");

            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("userEmail");
            entity.Property(e => e.AddressLine)
                .HasMaxLength(500)
                .HasColumnName("addressLine");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(200)
                .HasColumnName("avatarUrl");
            entity.Property(e => e.AvatarUrlPublicId)
                .HasMaxLength(200)
                .HasColumnName("avatarUrlPublicId");
            entity.Property(e => e.CityId).HasColumnName("cityId");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("longitude");
            entity.Property(e => e.SubDistrictId).HasColumnName("sub_district_id");

            entity.HasOne(d => d.City).WithMany(p => p.UserProfiles)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK__user_prof__cityI__5FB337D6");

            entity.HasOne(d => d.SubDistrict).WithMany(p => p.UserProfiles)
                .HasForeignKey(d => d.SubDistrictId)
                .HasConstraintName("FK__user_prof__sub_d__619B8048");

            entity.HasOne(d => d.UserEmailNavigation).WithOne(p => p.UserProfile)
                .HasForeignKey<UserProfile>(d => d.UserEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__user_prof__userE__628FA481");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
