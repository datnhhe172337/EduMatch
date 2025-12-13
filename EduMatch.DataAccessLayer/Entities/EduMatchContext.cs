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

    public virtual DbSet<Bank> Banks { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingNote> BookingNotes { get; set; }

    public virtual DbSet<BookingNoteMedium> BookingNoteMedia { get; set; }

    public virtual DbSet<BookingRefundRequest> BookingRefundRequests { get; set; }

    public virtual DbSet<CertificateType> CertificateTypes { get; set; }

    public virtual DbSet<CertificateTypeSubject> CertificateTypeSubjects { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatRoom> ChatRooms { get; set; }

    public virtual DbSet<ChatSession> ChatSessions { get; set; }

    public virtual DbSet<ChatbotMessage> ChatbotMessages { get; set; }

    public virtual DbSet<ClassRequest> ClassRequests { get; set; }

    public virtual DbSet<ClassRequestSlotsAvailability> ClassRequestSlotsAvailabilities { get; set; }

    public virtual DbSet<Deposit> Deposits { get; set; }

    public virtual DbSet<EducationInstitution> EducationInstitutions { get; set; }

    public virtual DbSet<FavoriteTutor> FavoriteTutors { get; set; }

    public virtual DbSet<FeedbackCriterion> FeedbackCriteria { get; set; }

    public virtual DbSet<GoogleToken> GoogleTokens { get; set; }

    public virtual DbSet<LearnerTrialLesson> LearnerTrialLessons { get; set; }

    public virtual DbSet<Level> Levels { get; set; }

    public virtual DbSet<MeetingSession> MeetingSessions { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Province> Provinces { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<RefundPolicy> RefundPolicies { get; set; }

    public virtual DbSet<RefundRequestEvidence> RefundRequestEvidences { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<ReportDefense> ReportDefenses { get; set; }

    public virtual DbSet<ReportEvidence> ReportEvidences { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<ScheduleChangeRequest> ScheduleChangeRequests { get; set; }

    public virtual DbSet<ScheduleCompletion> ScheduleCompletions { get; set; }

    public virtual DbSet<SubDistrict> SubDistricts { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<SystemFee> SystemFees { get; set; }

    public virtual DbSet<TimeSlot> TimeSlots { get; set; }

    public virtual DbSet<TutorApplication> TutorApplications { get; set; }

    public virtual DbSet<TutorAvailability> TutorAvailabilities { get; set; }

    public virtual DbSet<TutorCertificate> TutorCertificates { get; set; }

    public virtual DbSet<TutorEducation> TutorEducations { get; set; }

    public virtual DbSet<TutorFeedback> TutorFeedbacks { get; set; }

    public virtual DbSet<TutorFeedbackDetail> TutorFeedbackDetails { get; set; }

    public virtual DbSet<TutorPayout> TutorPayouts { get; set; }

    public virtual DbSet<TutorProfile> TutorProfiles { get; set; }

    public virtual DbSet<TutorRatingSummary> TutorRatingSummaries { get; set; }

    public virtual DbSet<TutorSubject> TutorSubjects { get; set; }

    public virtual DbSet<TutorVerificationRequest> TutorVerificationRequests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserBankAccount> UserBankAccounts { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

    public virtual DbSet<Withdrawal> Withdrawals { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__banks__3213E83FD8904DBA");

            entity.ToTable("banks");

            entity.HasIndex(e => e.Code, "UQ__banks__357D4CF9377AEDED").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(255)
                .HasColumnName("logoUrl");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.ShortName)
                .HasMaxLength(100)
                .HasColumnName("shortName");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bookings__3213E83F075704F9");

            entity.ToTable("bookings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookingDate).HasColumnName("bookingDate");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.LearnerEmail)
                .HasMaxLength(100)
                .HasColumnName("learnerEmail");
            entity.Property(e => e.PaymentStatus).HasColumnName("paymentStatus");
            entity.Property(e => e.RefundedAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("refundedAmount");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.SystemFeeAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("systemFeeAmount");
            entity.Property(e => e.SystemFeeId).HasColumnName("systemFeeId");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("totalAmount");
            entity.Property(e => e.TotalSessions)
                .HasDefaultValue(1)
                .HasColumnName("totalSessions");
            entity.Property(e => e.TutorReceiveAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("tutorReceiveAmount");
            entity.Property(e => e.TutorSubjectId).HasColumnName("tutorSubjectId");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("unitPrice");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

            entity.HasOne(d => d.LearnerEmailNavigation).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.LearnerEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Users");

            entity.HasOne(d => d.SystemFee).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.SystemFeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_bookings_system_fees");

            entity.HasOne(d => d.TutorSubject).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TutorSubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_TutorSubjects");
        });

        modelBuilder.Entity<BookingNote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__booking___3213E83FF7936004");

            entity.ToTable("booking_notes");

            entity.HasIndex(e => e.BookingId, "IX_booking_notes_booking_id");

            entity.HasIndex(e => e.CreatedByEmail, "IX_booking_notes_created_by_email");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Content)
                .HasMaxLength(2000)
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedByEmail)
                .HasMaxLength(100)
                .HasColumnName("created_by_email");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingNotes)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_booking_notes_bookings");
        });

        modelBuilder.Entity<BookingNoteMedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__booking___3213E83F3E0B1F1C");

            entity.ToTable("booking_note_media");

            entity.HasIndex(e => e.BookingNoteId, "IX_booking_note_media_note_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookingNoteId).HasColumnName("booking_note_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.FilePublicId)
                .HasMaxLength(255)
                .HasColumnName("file_public_id");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(500)
                .HasColumnName("file_url");
            entity.Property(e => e.MediaType).HasColumnName("media_type");

            entity.HasOne(d => d.BookingNote).WithMany(p => p.BookingNoteMedia)
                .HasForeignKey(d => d.BookingNoteId)
                .HasConstraintName("FK_booking_note_media_booking_notes");
        });

        modelBuilder.Entity<BookingRefundRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__booking___3213E83FD5E3B0DB");

            entity.ToTable("booking_refund_requests");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdminNote)
                .HasMaxLength(1000)
                .HasColumnName("adminNote");
            entity.Property(e => e.ApprovedAmount)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("approvedAmount");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.LearnerEmail)
                .HasMaxLength(100)
                .HasColumnName("learnerEmail");
            entity.Property(e => e.ProcessedAt).HasColumnName("processedAt");
            entity.Property(e => e.ProcessedBy)
                .HasMaxLength(100)
                .HasColumnName("processedBy");
            entity.Property(e => e.Reason)
                .HasMaxLength(1000)
                .HasColumnName("reason");
            entity.Property(e => e.RefundPolicyId).HasColumnName("refundPolicyId");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingRefundRequests)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BRR_Booking");

            entity.HasOne(d => d.LearnerEmailNavigation).WithMany(p => p.BookingRefundRequests)
                .HasForeignKey(d => d.LearnerEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BRR_Learner");

            entity.HasOne(d => d.RefundPolicy).WithMany(p => p.BookingRefundRequests)
                .HasForeignKey(d => d.RefundPolicyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BRR_RefundPolicy");
        });

        modelBuilder.Entity<CertificateType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__certific__3213E83FC9E555E2");

            entity.ToTable("certificate_types");

            entity.HasIndex(e => e.Verified, "IX_certificate_types_verified");

            entity.HasIndex(e => e.Code, "UQ__certific__357D4CF92A8BE488").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Verified).HasColumnName("verified");
            entity.Property(e => e.VerifiedAt).HasColumnName("verifiedAt");
            entity.Property(e => e.VerifiedBy)
                .HasMaxLength(255)
                .HasColumnName("verifiedBy");
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

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatSess__3214EC072E80C547");

            entity.ToTable("ChatSession");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UserEmail).HasMaxLength(100);

            entity.HasOne(d => d.UserEmailNavigation).WithMany(p => p.ChatSessions)
                .HasForeignKey(d => d.UserEmail)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ChatSession_User");
        });

        modelBuilder.Entity<ChatbotMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatbotM__3214EC07A61059F9");

            entity.ToTable("ChatbotMessage");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Role).HasMaxLength(20);

            entity.HasOne(d => d.Session).WithMany(p => p.ChatbotMessages)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChatMessage_ChatSession");
        });

        modelBuilder.Entity<ClassRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_re__3213E83F6A32D7FD");

            entity.ToTable("class_requests");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddressLine)
                .HasMaxLength(500)
                .HasColumnName("addressLine");
            entity.Property(e => e.ApprovedAt).HasColumnName("approvedAt");
            entity.Property(e => e.ApprovedBy)
                .HasMaxLength(255)
                .HasColumnName("approvedBy");
            entity.Property(e => e.CancelReason)
                .HasMaxLength(500)
                .HasColumnName("cancelReason");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.ExpectedSessions)
                .HasDefaultValue(1)
                .HasColumnName("expectedSessions");
            entity.Property(e => e.ExpectedStartDate).HasColumnName("expectedStartDate");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("latitude");
            entity.Property(e => e.LearnerEmail)
                .HasMaxLength(100)
                .HasColumnName("learnerEmail");
            entity.Property(e => e.LearningGoal)
                .HasMaxLength(300)
                .HasColumnName("learningGoal");
            entity.Property(e => e.LevelId).HasColumnName("levelId");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("longitude");
            entity.Property(e => e.Mode).HasColumnName("mode");
            entity.Property(e => e.ProvinceId).HasColumnName("provinceId");
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(500)
                .HasColumnName("rejectionReason");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.SubDistrictId).HasColumnName("sub_district_id");
            entity.Property(e => e.SubjectId).HasColumnName("subjectId");
            entity.Property(e => e.TargetUnitPriceMax)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("targetUnitPriceMax");
            entity.Property(e => e.TargetUnitPriceMin)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("targetUnitPriceMin");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
            entity.Property(e => e.TutorRequirement)
                .HasMaxLength(500)
                .HasColumnName("tutorRequirement");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

            entity.HasOne(d => d.LearnerEmailNavigation).WithMany(p => p.ClassRequests)
                .HasForeignKey(d => d.LearnerEmail)
                .HasConstraintName("FK_class_requests_users");

            entity.HasOne(d => d.Level).WithMany(p => p.ClassRequests)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_class_requests_levels");

            entity.HasOne(d => d.Province).WithMany(p => p.ClassRequests)
                .HasForeignKey(d => d.ProvinceId)
                .HasConstraintName("FK_class_requests_provinces");

            entity.HasOne(d => d.SubDistrict).WithMany(p => p.ClassRequests)
                .HasForeignKey(d => d.SubDistrictId)
                .HasConstraintName("FK_class_requests_sub_district");

            entity.HasOne(d => d.Subject).WithMany(p => p.ClassRequests)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_class_requests_subjects");
        });

        modelBuilder.Entity<ClassRequestSlotsAvailability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__class_re__3213E83F6E46DF3C");

            entity.ToTable("class_request_slots_availability");

            entity.HasIndex(e => new { e.ClassRequestId, e.DayOfWeek, e.SlotId }, "UQ_class_request_slot").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassRequestId).HasColumnName("classRequestId");
            entity.Property(e => e.DayOfWeek).HasColumnName("dayOfWeek");
            entity.Property(e => e.SlotId).HasColumnName("slotId");

            entity.HasOne(d => d.ClassRequest).WithMany(p => p.ClassRequestSlotsAvailabilities)
                .HasForeignKey(d => d.ClassRequestId)
                .HasConstraintName("FK_class_request_slots_class_requests");

            entity.HasOne(d => d.Slot).WithMany(p => p.ClassRequestSlotsAvailabilities)
                .HasForeignKey(d => d.SlotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_class_request_slots_time_slots");
        });

        modelBuilder.Entity<Deposit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__deposits__3213E83F5D423D1C");

            entity.ToTable("deposits");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CompletedAt).HasColumnName("completedAt");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.GatewayTransactionCode)
                .HasMaxLength(100)
                .HasColumnName("gatewayTransactionCode");
            entity.Property(e => e.PaymentGateway)
                .HasMaxLength(50)
                .HasColumnName("paymentGateway");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.WalletId).HasColumnName("walletId");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_deposits_wallets");
        });

        modelBuilder.Entity<EducationInstitution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__educatio__3213E83FFF7331E8");

            entity.ToTable("education_institutions");

            entity.HasIndex(e => e.Verified, "IX_education_institutions_verified");

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
            entity.Property(e => e.Verified).HasColumnName("verified");
            entity.Property(e => e.VerifiedAt).HasColumnName("verifiedAt");
            entity.Property(e => e.VerifiedBy)
                .HasMaxLength(255)
                .HasColumnName("verifiedBy");
        });

        modelBuilder.Entity<FavoriteTutor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__favorite__3213E83F5994F48E");

            entity.ToTable("favorite_tutors");

            entity.HasIndex(e => new { e.UserEmail, e.TutorId }, "UQ_favorite_tutors_user_tutor").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.TutorId).HasColumnName("tutor_id");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("user_email");

            entity.HasOne(d => d.Tutor).WithMany(p => p.FavoriteTutors)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_favorite_tutors_tutor_profiles");

            entity.HasOne(d => d.UserEmailNavigation).WithMany(p => p.FavoriteTutors)
                .HasForeignKey(d => d.UserEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_favorite_tutors_users");
        });

        modelBuilder.Entity<FeedbackCriterion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3214EC0700F318AC");

            entity.HasIndex(e => e.Code, "UQ__Feedback__A25C5AA767EB3903").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<GoogleToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__google_t__3213E83FBEA09237");

            entity.ToTable("google_tokens");

            entity.HasIndex(e => e.AccountEmail, "UQ__google_t__CA548954DB070980").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessToken).HasColumnName("accessToken");
            entity.Property(e => e.AccountEmail)
                .HasMaxLength(255)
                .HasColumnName("accountEmail");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.ExpiresAt).HasColumnName("expiresAt");
            entity.Property(e => e.RefreshToken).HasColumnName("refreshToken");
            entity.Property(e => e.Scope)
                .HasMaxLength(500)
                .HasColumnName("scope");
            entity.Property(e => e.TokenType)
                .HasMaxLength(50)
                .HasColumnName("tokenType");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
        });

        modelBuilder.Entity<LearnerTrialLesson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__learner___3213E83FFA0F7126");

            entity.ToTable("learner_trial_lessons");

            entity.HasIndex(e => new { e.LearnerEmail, e.TutorId, e.SubjectId }, "UQ_learner_trial_lessons_user_tutor_subject").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.LearnerEmail)
                .HasMaxLength(100)
                .HasColumnName("learner_email");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.TutorId).HasColumnName("tutor_id");

            entity.HasOne(d => d.LearnerEmailNavigation).WithMany(p => p.LearnerTrialLessons)
                .HasForeignKey(d => d.LearnerEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_learner_trial_lessons_users");

            entity.HasOne(d => d.Subject).WithMany(p => p.LearnerTrialLessons)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_learner_trial_lessons_subjects");

            entity.HasOne(d => d.Tutor).WithMany(p => p.LearnerTrialLessons)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_learner_trial_lessons_tutor_profiles");
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

        modelBuilder.Entity<MeetingSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__meeting___3213E83FA5F10641");

            entity.ToTable("meeting_sessions");

            entity.HasIndex(e => e.ScheduleId, "UQ_meeting_sessions_scheduleId").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.EndTime).HasColumnName("endTime");
            entity.Property(e => e.EventId)
                .HasMaxLength(200)
                .HasColumnName("eventId");
            entity.Property(e => e.MeetCode)
                .HasMaxLength(100)
                .HasColumnName("meetCode");
            entity.Property(e => e.MeetLink)
                .HasMaxLength(500)
                .HasColumnName("meetLink");
            entity.Property(e => e.MeetingType).HasColumnName("meetingType");
            entity.Property(e => e.OrganizerEmail)
                .HasMaxLength(255)
                .HasColumnName("organizerEmail");
            entity.Property(e => e.ScheduleId).HasColumnName("scheduleId");
            entity.Property(e => e.StartTime).HasColumnName("startTime");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

            entity.HasOne(d => d.OrganizerEmailNavigation).WithMany(p => p.MeetingSessions)
                .HasPrincipalKey(p => p.AccountEmail)
                .HasForeignKey(d => d.OrganizerEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Meeting_GoogleToken");

            entity.HasOne(d => d.Schedule).WithOne(p => p.MeetingSession)
                .HasForeignKey<MeetingSession>(d => d.ScheduleId)
                .HasConstraintName("FK_meeting_sessions_schedule");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            entity.HasIndex(e => new { e.UserEmail, e.IsRead }, "IX_notifications_user_isRead");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsRead).HasColumnName("isRead");
            entity.Property(e => e.LinkUrl)
                .HasMaxLength(500)
                .HasColumnName("linkUrl");
            entity.Property(e => e.Message)
                .HasMaxLength(500)
                .HasColumnName("message");
            entity.Property(e => e.ReadAt)
                .HasColumnType("datetime")
                .HasColumnName("readAt");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("userEmail");

            entity.HasOne(d => d.UserEmailNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_notifications_users");
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

        modelBuilder.Entity<RefundPolicy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__refund_p__3213E83F5386015A");

            entity.ToTable("refund_policies");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("createdBy");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.RefundPercentage)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("refundPercentage");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updatedBy");
        });

        modelBuilder.Entity<RefundRequestEvidence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__refund_r__3213E83F784C9E17");

            entity.ToTable("refund_request_evidences");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookingRefundRequestId).HasColumnName("bookingRefundRequestId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(500)
                .HasColumnName("fileUrl");

            entity.HasOne(d => d.BookingRefundRequest).WithMany(p => p.RefundRequestEvidences)
                .HasForeignKey(d => d.BookingRefundRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RRE_BookingRefundRequest");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("reports");

            entity.HasIndex(e => e.BookingId, "IX_reports_bookingId");

            entity.HasIndex(e => e.ScheduleId, "IX_reports_scheduleId");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdminNotes).HasColumnName("adminNotes");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.HandledByAdminEmail)
                .HasMaxLength(100)
                .HasColumnName("handledByAdminEmail");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.ReportedUserEmail)
                .HasMaxLength(100)
                .HasColumnName("reportedUserEmail");
            entity.Property(e => e.ReporterUserEmail)
                .HasMaxLength(100)
                .HasColumnName("reporterUserEmail");
            entity.Property(e => e.ScheduleId).HasColumnName("scheduleId");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TutorDefenseNote).HasColumnName("tutorDefenseNote");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Booking).WithMany(p => p.Reports)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_reports_bookings");

            entity.HasOne(d => d.HandledByAdminEmailNavigation).WithMany(p => p.ReportHandledByAdminEmailNavigations)
                .HasForeignKey(d => d.HandledByAdminEmail)
                .HasConstraintName("FK_reports_admin_users");

            entity.HasOne(d => d.ReportedUserEmailNavigation).WithMany(p => p.ReportReportedUserEmailNavigations)
                .HasForeignKey(d => d.ReportedUserEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_reports_reported_users");

            entity.HasOne(d => d.ReporterUserEmailNavigation).WithMany(p => p.ReportReporterUserEmailNavigations)
                .HasForeignKey(d => d.ReporterUserEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_reports_reporter_users");

            entity.HasOne(d => d.Schedule).WithMany(p => p.Reports).HasForeignKey(d => d.ScheduleId);
        });

        modelBuilder.Entity<ReportDefense>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__report_d__3213E83F1FB59267");

            entity.ToTable("report_defenses");

            entity.HasIndex(e => e.ReportId, "IX_report_defenses_reportId");

            entity.HasIndex(e => e.TutorEmail, "IX_report_defenses_tutorEmail");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.Note)
                .HasMaxLength(2000)
                .HasColumnName("note");
            entity.Property(e => e.ReportId).HasColumnName("reportId");
            entity.Property(e => e.TutorEmail)
                .HasMaxLength(100)
                .HasColumnName("tutorEmail");

            entity.HasOne(d => d.Report).WithMany(p => p.ReportDefenses)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("FK_report_defenses_reports");
        });

        modelBuilder.Entity<ReportEvidence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__report_e__3213E83F9AC6F218");

            entity.ToTable("report_evidences");

            entity.HasIndex(e => e.ReportId, "IX_report_evidences_reportId");

            entity.HasIndex(e => e.SubmittedByEmail, "IX_report_evidences_submittedByEmail");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Caption)
                .HasMaxLength(255)
                .HasColumnName("caption");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.DefenseId).HasColumnName("defenseId");
            entity.Property(e => e.EvidenceType).HasColumnName("evidenceType");
            entity.Property(e => e.FilePublicId)
                .HasMaxLength(255)
                .HasColumnName("filePublicId");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(500)
                .HasColumnName("fileUrl");
            entity.Property(e => e.MediaType).HasColumnName("mediaType");
            entity.Property(e => e.ReportId).HasColumnName("reportId");
            entity.Property(e => e.SubmittedByEmail)
                .HasMaxLength(100)
                .HasColumnName("submittedByEmail");

            entity.HasOne(d => d.Defense).WithMany(p => p.ReportEvidences)
                .HasForeignKey(d => d.DefenseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_report_evidences_report_defenses");

            entity.HasOne(d => d.Report).WithMany(p => p.ReportEvidences)
                .HasForeignKey(d => d.ReportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_report_evidences_reports");
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

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__schedule__3213E83F34551DEA");

            entity.ToTable("schedule");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttendanceNote)
                .HasMaxLength(500)
                .HasColumnName("attendanceNote");
            entity.Property(e => e.AvailabilitiId).HasColumnName("availabilitiId");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.IsRefunded).HasColumnName("isRefunded");
            entity.Property(e => e.RefundedAt).HasColumnName("refundedAt");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

            entity.HasOne(d => d.Availabiliti).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.AvailabilitiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedule_Availability");

            entity.HasOne(d => d.Booking).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedule_Bookings");
        });

        modelBuilder.Entity<ScheduleChangeRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__schedule__3213E83FB88FCF3B");

            entity.ToTable("schedule_change_requests");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.NewAvailabilitiId).HasColumnName("newAvailabilitiId");
            entity.Property(e => e.OldAvailabilitiId).HasColumnName("oldAvailabilitiId");
            entity.Property(e => e.ProcessedAt).HasColumnName("processedAt");
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .HasColumnName("reason");
            entity.Property(e => e.RequestedToEmail)
                .HasMaxLength(100)
                .HasColumnName("requestedToEmail");
            entity.Property(e => e.RequesterEmail)
                .HasMaxLength(100)
                .HasColumnName("requesterEmail");
            entity.Property(e => e.ScheduleId).HasColumnName("scheduleId");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.NewAvailabiliti).WithMany(p => p.ScheduleChangeRequestNewAvailabilitis)
                .HasForeignKey(d => d.NewAvailabilitiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SCR_NewAvail");

            entity.HasOne(d => d.OldAvailabiliti).WithMany(p => p.ScheduleChangeRequestOldAvailabilitis)
                .HasForeignKey(d => d.OldAvailabilitiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SCR_OldAvail");

            entity.HasOne(d => d.RequestedToEmailNavigation).WithMany(p => p.ScheduleChangeRequestRequestedToEmailNavigations)
                .HasForeignKey(d => d.RequestedToEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SCR_RequestedTo");

            entity.HasOne(d => d.RequesterEmailNavigation).WithMany(p => p.ScheduleChangeRequestRequesterEmailNavigations)
                .HasForeignKey(d => d.RequesterEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SCR_Requester");

            entity.HasOne(d => d.Schedule).WithMany(p => p.ScheduleChangeRequests)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SCR_Schedule");
        });

        modelBuilder.Entity<ScheduleCompletion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__schedule__3213E83FC6A0EEE5");

            entity.ToTable("schedule_completions");

            entity.HasIndex(e => e.BookingId, "IX_schedule_completions_booking");

            entity.HasIndex(e => new { e.Status, e.ConfirmationDeadline }, "IX_schedule_completions_status_deadline");

            entity.HasIndex(e => e.ScheduleId, "UQ_schedule_completions_schedule").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AutoCompletedAt)
                .HasPrecision(0)
                .HasColumnName("auto_completed_at");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.ConfirmationDeadline)
                .HasPrecision(0)
                .HasColumnName("confirmation_deadline");
            entity.Property(e => e.ConfirmedAt)
                .HasPrecision(0)
                .HasColumnName("confirmed_at");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.LearnerEmail)
                .HasMaxLength(100)
                .HasColumnName("learner_email");
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("note");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TutorId).HasColumnName("tutor_id");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Booking).WithMany(p => p.ScheduleCompletions)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_schedule_completions_booking");

            entity.HasOne(d => d.LearnerEmailNavigation).WithMany(p => p.ScheduleCompletions)
                .HasForeignKey(d => d.LearnerEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_schedule_completions_learner");

            entity.HasOne(d => d.Report).WithMany(p => p.ScheduleCompletions)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("FK_schedule_completions_report");

            entity.HasOne(d => d.Schedule).WithOne(p => p.ScheduleCompletion)
                .HasForeignKey<ScheduleCompletion>(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_schedule_completions_schedule");

            entity.HasOne(d => d.Tutor).WithMany(p => p.ScheduleCompletions)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_schedule_completions_tutor");
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

        modelBuilder.Entity<SystemFee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__system_f__3213E83F4A31982F");

            entity.ToTable("system_fees");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.EffectiveFrom).HasColumnName("effectiveFrom");
            entity.Property(e => e.EffectiveTo).HasColumnName("effectiveTo");
            entity.Property(e => e.FixedAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("fixedAmount");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Percentage)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("percentage");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__time_slo__3213E83F26A39A86");

            entity.ToTable("time_slots");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndTime).HasColumnName("endTime");
            entity.Property(e => e.StartTime).HasColumnName("startTime");
        });

        modelBuilder.Entity<TutorApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_ap__3213E83F85FF0752");

            entity.ToTable("tutor_application");

            entity.HasIndex(e => new { e.ClassRequestId, e.TutorId }, "UQ_tutor_application").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassRequestId).HasColumnName("classRequestId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TutorId).HasColumnName("tutorId");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

            entity.HasOne(d => d.ClassRequest).WithMany(p => p.TutorApplications)
                .HasForeignKey(d => d.ClassRequestId)
                .HasConstraintName("FK_tutor_application_class_request");

            entity.HasOne(d => d.Tutor).WithMany(p => p.TutorApplications)
                .HasForeignKey(d => d.TutorId)
                .HasConstraintName("FK_tutor_application_tutor_profiles");
        });

        modelBuilder.Entity<TutorAvailability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_av__3213E83FAAA63124");

            entity.ToTable("tutor_availabilities");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.EndDate).HasColumnName("endDate");
            entity.Property(e => e.SlotId).HasColumnName("slotId");
            entity.Property(e => e.StartDate).HasColumnName("startDate");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TutorId).HasColumnName("tutorId");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

            entity.HasOne(d => d.Slot).WithMany(p => p.TutorAvailabilities)
                .HasForeignKey(d => d.SlotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tutor_availabilities_time_slots");

            entity.HasOne(d => d.Tutor).WithMany(p => p.TutorAvailabilities)
                .HasForeignKey(d => d.TutorId)
                .HasConstraintName("FK_tutor_availabilities_tutor_profiles");
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

        modelBuilder.Entity<TutorFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TutorFee__3214EC07BA27576C");

            entity.ToTable("TutorFeedback");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.LearnerEmail).HasMaxLength(100);
        });

        modelBuilder.Entity<TutorFeedbackDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TutorFee__3214EC077C9D771F");

            entity.HasOne(d => d.Criterion).WithMany(p => p.TutorFeedbackDetails)
                .HasForeignKey(d => d.CriterionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TutorFeedbackDetails_Criteria");

            entity.HasOne(d => d.Feedback).WithMany(p => p.TutorFeedbackDetails)
                .HasForeignKey(d => d.FeedbackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TutorFeedbackDetails_Feedback");
        });

        modelBuilder.Entity<TutorPayout>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_pa__3213E83F6BFFF89C");

            entity.ToTable("tutor_payouts");

            entity.HasIndex(e => new { e.Status, e.ScheduledPayoutDate }, "IX_tutor_payouts_status_date");

            entity.HasIndex(e => new { e.TutorWalletId, e.Status }, "IX_tutor_payouts_wallet_status");

            entity.HasIndex(e => e.ScheduleId, "UQ_tutor_payouts_schedule").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.HoldReason)
                .HasMaxLength(255)
                .HasColumnName("hold_reason");
            entity.Property(e => e.PayoutTrigger).HasColumnName("payout_trigger");
            entity.Property(e => e.ReleasedAt)
                .HasPrecision(0)
                .HasColumnName("released_at");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.ScheduledPayoutDate).HasColumnName("scheduled_payout_date");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.SystemFeeAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("system_fee_amount");
            entity.Property(e => e.TutorWalletId).HasColumnName("tutor_wallet_id");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasColumnName("updated_at");
            entity.Property(e => e.WalletTransactionId).HasColumnName("wallet_transaction_id");

            entity.HasOne(d => d.Booking).WithMany(p => p.TutorPayouts)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tutor_payouts_booking");

            entity.HasOne(d => d.Schedule).WithOne(p => p.TutorPayout)
                .HasForeignKey<TutorPayout>(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tutor_payouts_schedule");

            entity.HasOne(d => d.TutorWallet).WithMany(p => p.TutorPayouts)
                .HasForeignKey(d => d.TutorWalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tutor_payouts_wallet");

            entity.HasOne(d => d.WalletTransaction).WithMany(p => p.TutorPayouts)
                .HasForeignKey(d => d.WalletTransactionId)
                .HasConstraintName("FK_tutor_payouts_tx");
        });

        modelBuilder.Entity<TutorProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_pr__3213E83F924DBCEA");

            entity.ToTable("tutor_profiles");

            entity.HasIndex(e => e.UserEmail, "UQ__tutor_pr__D54ADF55BAF574EE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.LastSync).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TeachingExp)
                .HasMaxLength(500)
                .HasColumnName("teachingExp");
            entity.Property(e => e.TeachingModes).HasColumnName("teachingModes");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("userEmail");
            entity.Property(e => e.VerifiedAt).HasColumnName("verifiedAt");
            entity.Property(e => e.VerifiedBy)
                .HasMaxLength(255)
                .HasColumnName("verifiedBy");
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

        modelBuilder.Entity<TutorRatingSummary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TutorRat__3214EC079B284417");

            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Tutor).WithMany(p => p.TutorRatingSummaries)
                .HasForeignKey(d => d.TutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TutorRatingSummary_Tutor");
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

        modelBuilder.Entity<TutorVerificationRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tutor_ve__3213E83FB0528B3E");

            entity.ToTable("tutor_verification_requests");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdminNote)
                .HasMaxLength(1000)
                .HasColumnName("adminNote");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");
            entity.Property(e => e.ProcessedAt).HasColumnName("processedAt");
            entity.Property(e => e.ProcessedBy)
                .HasMaxLength(100)
                .HasColumnName("processedBy");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TutorId).HasColumnName("tutorId");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("userEmail");

            entity.HasOne(d => d.Tutor).WithMany(p => p.TutorVerificationRequests)
                .HasForeignKey(d => d.TutorId)
                .HasConstraintName("FK_TVR_Tutor");
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

        modelBuilder.Entity<UserBankAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_ban__3213E83F754938F6");

            entity.ToTable("user_bank_accounts");

            entity.HasIndex(e => new { e.UserEmail, e.BankId, e.AccountNumber }, "UQ_user_bank_accounts").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountHolderName)
                .HasMaxLength(200)
                .HasColumnName("accountHolderName");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(50)
                .HasColumnName("accountNumber");
            entity.Property(e => e.BankId).HasColumnName("bankId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsDefault).HasColumnName("isDefault");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("userEmail");

            entity.HasOne(d => d.Bank).WithMany(p => p.UserBankAccounts)
                .HasForeignKey(d => d.BankId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_user_bank_accounts_banks");

            entity.HasOne(d => d.UserEmailNavigation).WithMany(p => p.UserBankAccounts)
                .HasForeignKey(d => d.UserEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_user_bank_accounts_users");
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

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__wallets__3213E83F7C1679AB");

            entity.ToTable("wallets");

            entity.HasIndex(e => e.UserEmail, "UQ__wallets__D54ADF5592575686").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.LockedBalance)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("lockedBalance");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(100)
                .HasColumnName("userEmail");

            entity.HasOne(d => d.UserEmailNavigation).WithOne(p => p.Wallet)
                .HasForeignKey<Wallet>(d => d.UserEmail)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_wallets_users");
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__wallet_t__3213E83F76711A91");

            entity.ToTable("wallet_transactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BalanceAfter)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("balanceAfter");
            entity.Property(e => e.BalanceBefore)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("balanceBefore");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.DepositId).HasColumnName("depositId");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.ReferenceCode)
                .HasMaxLength(100)
                .HasColumnName("referenceCode");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TransactionType).HasColumnName("transactionType");
            entity.Property(e => e.WalletId).HasColumnName("walletId");
            entity.Property(e => e.WithdrawalId).HasColumnName("withdrawalId");

            entity.HasOne(d => d.Booking).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_wallet_transactions_bookings");

            entity.HasOne(d => d.Deposit).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.DepositId)
                .HasConstraintName("FK_wallet_transactions_deposits");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_wallet_transactions_wallets");

            entity.HasOne(d => d.Withdrawal).WithMany(p => p.WalletTransactions)
                .HasForeignKey(d => d.WithdrawalId)
                .HasConstraintName("FK_wallet_transactions_withdrawals");
        });

        modelBuilder.Entity<Withdrawal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__withdraw__3213E83FF2862746");

            entity.ToTable("withdrawals");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdminEmail)
                .HasMaxLength(100)
                .HasColumnName("adminEmail");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CompletedAt).HasColumnName("completedAt");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("createdAt");
            entity.Property(e => e.ProcessedAt).HasColumnName("processedAt");
            entity.Property(e => e.RejectReason)
                .HasMaxLength(500)
                .HasColumnName("rejectReason");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UserBankAccountId).HasColumnName("userBankAccountId");
            entity.Property(e => e.WalletId).HasColumnName("walletId");

            entity.HasOne(d => d.UserBankAccount).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.UserBankAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_withdrawals_user_bank_accounts");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_withdrawals_wallets");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
