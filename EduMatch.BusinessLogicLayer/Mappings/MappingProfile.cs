using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using EduMatch.BusinessLogicLayer.Requests.TutorCertificate;
using EduMatch.BusinessLogicLayer.Requests.TutorEducation;
using EduMatch.BusinessLogicLayer.Requests.TutorSubject;
using EduMatch.BusinessLogicLayer.Requests.TutorAvailability;
using EduMatch.BusinessLogicLayer.Requests.User;
using EduMatch.BusinessLogicLayer.Requests.CertificateType;
using EduMatch.BusinessLogicLayer.Requests.Level;
using EduMatch.BusinessLogicLayer.Requests.Subject;
using EduMatch.BusinessLogicLayer.Requests.TimeSlot;
using EduMatch.BusinessLogicLayer.Requests.ScheduleChangeRequest;
using EduMatch.BusinessLogicLayer.Requests.RefundRequestEvidence;

using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Linq; // Added for .Select
using System.Collections.Generic; // Added for List

namespace EduMatch.BusinessLogicLayer.Mappings
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // CertificateType mappings
            CreateMap<CertificateType, CertificateTypeDto>()
                .ForMember(d => d.Subjects, o => o.MapFrom(s =>
                    s.CertificateTypeSubjects.Select(x => new SubjectDto
                    {
                        Id = x.Subject.Id,
                        SubjectName = x.Subject.SubjectName,
                        CertificateTypes = new List<CertificateTypeDto>()
                    })
                ));

            CreateMap<CertificateTypeCreateRequest, CertificateType>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<CertificateTypeUpdateRequest, CertificateType>();

            // CertificateTypeSubject mappings
            CreateMap<CertificateTypeSubject, CertificateTypeSubjectDto>().ReverseMap();

            // EducationInstitution mappings
            CreateMap<EducationInstitution, EducationInstitutionDto>().ReverseMap();

            // Level mappings
            CreateMap<Level, LevelDto>().ReverseMap();
            CreateMap<LevelCreateRequest, Level>();
            CreateMap<LevelUpdateRequest, Level>();

            // Subject mappings
            CreateMap<Subject, SubjectDto>()
                .ForMember(dest => dest.CertificateTypes, opt => opt.MapFrom(src => src.CertificateTypeSubjects.Select(cts => cts.CertificateType)));

            CreateMap<SubjectCreateRequest, Subject>();
            CreateMap<SubjectUpdateRequest, Subject>();

            // TimeSlot mappings
            CreateMap<TimeSlot, TimeSlotDto>().ReverseMap();
            CreateMap<TimeSlotCreateRequest, TimeSlot>();
            CreateMap<TimeSlotUpdateRequest, TimeSlot>();

            // TutorAvailability mappings
            CreateMap<TutorAvailability, TutorAvailabilityDto>()
                .ForMember(dest => dest.Slot, opt => opt.MapFrom(src => src.Slot))
                .ForMember(dest => dest.Tutor, opt => opt.MapFrom(src => src.Tutor))
                .ReverseMap();
            CreateMap<TutorAvailabilityCreateRequest, TutorAvailability>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Slot, opt => opt.Ignore())
                .ForMember(dest => dest.Tutor, opt => opt.Ignore());
            CreateMap<TutorAvailabilityUpdateRequest, TutorAvailability>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Slot, opt => opt.Ignore())
                .ForMember(dest => dest.Tutor, opt => opt.Ignore());

            // TutorCertificate mappings
            CreateMap<TutorCertificate, TutorCertificateDto>().ReverseMap();
            CreateMap<TutorEducation, TutorEducationDto>().ReverseMap();


            // CertificateType mappings
			CreateMap<CertificateType, CertificateTypeDto>()
				.ForMember(dest => dest.Verified, opt => opt.MapFrom(src => (VerifyStatus)src.Verified))
				.ForMember(d => d.Subjects, o => o.MapFrom(s =>
					s.CertificateTypeSubjects.Select(x => new SubjectDto
					{
						Id = x.Subject.Id,
						SubjectName = x.Subject.SubjectName,
						CertificateTypes = new List<CertificateTypeDto>()
					})
				));	

            // TutorProfile mappings
            CreateMap<TutorProfile, TutorProfileDto>()
                .ForMember(d => d.TutorAvailabilities,
                    opt => opt.MapFrom(s => s.TutorAvailabilities != null
                        ? s.TutorAvailabilities.Select(a => new TutorAvailabilityDto
                        {
                            Id = a.Id,
                            StartDate = a.StartDate,
                            EndDate = a.EndDate,
                            Status =(TutorAvailabilityStatus) a.Status,
                            CreatedAt = a.CreatedAt,
                            UpdatedAt = a.UpdatedAt,
                            Slot = a.Slot != null
                                ? new TimeSlotDto { Id = a.Slot.Id, StartTime = a.Slot.StartTime, EndTime = a.Slot.EndTime }
                                : null
                        })
                        : new List<TutorAvailabilityDto>()))
                .ForMember(d => d.TutorCertificates,
                    opt => opt.MapFrom(s => s.TutorCertificates != null
                        ? s.TutorCertificates.Select(c => new TutorCertificateDto
                        {
                            Id = c.Id,
                            IssueDate = c.IssueDate,
                            ExpiryDate = c.ExpiryDate,
                            CreatedAt = c.CreatedAt,
                            Verified =(VerifyStatus) c.Verified,
                            CertificateUrl = c.CertificateUrl,
                            CertificateType = c.CertificateType != null
                                ? new CertificateTypeDto { Id = c.CertificateType.Id, Name = c.CertificateType.Name }
                                : null
                        })
                        : new List<TutorCertificateDto>()))
                .ForMember(d => d.TutorEducations,
                    opt => opt.MapFrom(s => s.TutorEducations != null
                        ? s.TutorEducations.Select(e => new TutorEducationDto
                        {
                            Id = e.Id,
                            CertificateUrl = e.CertificateUrl,
                            IssueDate = e.IssueDate,
                            CreatedAt = e.CreatedAt,
                            Verified = (VerifyStatus)e.Verified,
                            Institution = e.Institution != null
                                ? new EducationInstitutionDto { Id = e.Institution.Id, Name = e.Institution.Name }
                                : null
                        })
                        : new List<TutorEducationDto>()))
                .ForMember(d => d.TutorSubjects,
                    opt => opt.MapFrom(s => s.TutorSubjects != null
                        ? s.TutorSubjects.Select(ts => new TutorSubjectDto
                        {
                            Id = ts.Id,
                            HourlyRate = ts.HourlyRate,
                            Subject = ts.Subject != null
                                ? new SubjectDto { Id = ts.Subject.Id, SubjectName = ts.Subject.SubjectName }
                                : null,
                            Level = ts.Level != null
                                ? new LevelDto { Id = ts.Level.Id, Name = ts.Level.Name }
                                : null
                        })
                        : new List<TutorSubjectDto>()))
                // mapping from UserProfile
                .ForMember(d => d.Gender, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.Gender))
                .ForMember(d => d.Province, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.City))
                .ForMember(d => d.SubDistrict, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.SubDistrict))
                .ForMember(d => d.Dob, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.Dob))
                .ForMember(d => d.AvatarUrl, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.AvatarUrl))
                .ForMember(d => d.AddressLine, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.AddressLine))
                .ForMember(d => d.Latitude, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.Latitude))
                .ForMember(d => d.Longitude, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.Longitude))
                // mapping to User 
                .ForMember(d => d.UserName, opt => opt.MapFrom(src => src.UserEmailNavigation.UserName))
                .ForMember(d => d.Phone, opt => opt.MapFrom(src => src.UserEmailNavigation.Phone));

            // TutorSubject mappings
            CreateMap<TutorSubject, TutorSubjectDto>().ReverseMap();
            CreateMap<TutorSubjectCreateRequest, TutorSubject>();
            CreateMap<TutorSubjectUpdateRequest, TutorSubject>(); // This is your existing one

            // User mappings
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<UserUpdateRequest, User>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Role mappings
            CreateMap<Role, RoleDto>().ReverseMap();
            CreateMap<RefreshToken, RefreshTokenDto>().ReverseMap();
            CreateMap<UserProfile, UserProfileDto>().ReverseMap();
            CreateMap<UserProfileUpdateRequest, UserProfile>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Province mappings
            CreateMap<Province, ProvinceDto>();
            CreateMap<SubDistrict, SubDistrictDto>();

          

        

			// EducationInstitution mappings
			CreateMap<EducationInstitution, EducationInstitutionDto>()
				.ForMember(dest => dest.InstitutionType, opt => opt.MapFrom(src => (InstitutionType)src.InstitutionType))
				.ForMember(dest => dest.Verified, opt => opt.MapFrom(src => (VerifyStatus)src.Verified));



			// Level mappings
			CreateMap<Level, LevelDto>().ReverseMap();
			CreateMap<LevelCreateRequest, Level>();
			CreateMap<LevelUpdateRequest, Level>();

			// Subject mappings
			CreateMap<Subject, SubjectDto>()
				.ForMember(dest => dest.CertificateTypes, opt => opt.MapFrom(src => src.CertificateTypeSubjects.Select(cts => cts.CertificateType)));

			CreateMap<SubjectCreateRequest, Subject>();
			CreateMap<SubjectUpdateRequest, Subject>();

			// TimeSlot mappings
			CreateMap<TimeSlot, TimeSlotDto>().ReverseMap();
			CreateMap<TimeSlotCreateRequest, TimeSlot>();
			CreateMap<TimeSlotUpdateRequest, TimeSlot>();

            // TutorAvailability mappings
            CreateMap<TutorAvailability, TutorAvailabilityDto>()
                .ForMember(dest => dest.Slot, opt => opt.MapFrom(src => src.Slot))
                .ForMember(dest => dest.Tutor, opt => opt.MapFrom(src => src.Tutor))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (TutorAvailabilityStatus)src.Status));


			// TutorCertificate mappings
			CreateMap<TutorCertificate, TutorCertificateDto>()
				.ForMember(dest => dest.Verified, opt => opt.MapFrom(src => (VerifyStatus)src.Verified))
				.ForMember(dest => dest.CertificateType, opt => opt.MapFrom(src => src.CertificateType));

			// TutorEducation mappings
			CreateMap<TutorEducation, TutorEducationDto>()
				.ForMember(dest => dest.Verified, opt => opt.MapFrom(src => (VerifyStatus)src.Verified))
				.ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.Institution));


			// TutorProfile mappings
			// ========== PROFILE ==========
			CreateMap<TutorProfile, TutorProfileDto>()
				.ForMember(dest => dest.TeachingModes, opt => opt.MapFrom(src => (TeachingMode)src.TeachingModes))
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => (TutorStatus)src.Status))
				.ForMember(d => d.TutorAvailabilities,
					opt => opt.MapFrom(s => s.TutorAvailabilities != null
						? s.TutorAvailabilities.Select(a => new TutorAvailabilityDto
						{
							Id = a.Id,
							StartDate = a.StartDate,
							EndDate = a.EndDate,
							Status = (TutorAvailabilityStatus?)a.Status,
							CreatedAt = a.CreatedAt,
							UpdatedAt = a.UpdatedAt,
							Slot = a.Slot != null
								? new TimeSlotDto { Id = a.Slot.Id, StartTime = a.Slot.StartTime, EndTime = a.Slot.EndTime }
								: null
						})
						: new List<TutorAvailabilityDto>()))

				.ForMember(d => d.TutorCertificates,
					opt => opt.MapFrom(s => s.TutorCertificates != null
						? s.TutorCertificates.Select(c => new TutorCertificateDto
						{
							Id = c.Id,
							IssueDate = c.IssueDate,
							ExpiryDate = c.ExpiryDate,
							CreatedAt = c.CreatedAt,
							Verified = (VerifyStatus)c.Verified,
							CertificateUrl = c.CertificateUrl,
							RejectReason = c.RejectReason,
							CertificateType = c.CertificateType != null
								? new CertificateTypeDto { Id = c.CertificateType.Id, Name = c.CertificateType.Name }
								: null
						})
						: new List<TutorCertificateDto>()))
				.ForMember(d => d.TutorEducations,
					opt => opt.MapFrom(s => s.TutorEducations != null
						? s.TutorEducations.Select(e => new TutorEducationDto
						{
							Id = e.Id,
							CertificateUrl = e.CertificateUrl,
							IssueDate = e.IssueDate,
							CreatedAt = e.CreatedAt,
							Verified = (VerifyStatus)e.Verified,
							RejectReason = e.RejectReason,

							Institution = e.Institution != null
								? new EducationInstitutionDto { Id = e.Institution.Id, Name = e.Institution.Name }
								: null
						})
						: new List<TutorEducationDto>()))
				.ForMember(d => d.TutorSubjects,
					opt => opt.MapFrom(s => s.TutorSubjects != null
						? s.TutorSubjects.Select(ts => new TutorSubjectDto
						{
							Id = ts.Id,
							HourlyRate = ts.HourlyRate,
							Subject = ts.Subject != null
								? new SubjectDto { Id = ts.Subject.Id, SubjectName = ts.Subject.SubjectName }
								: null,
							Level = ts.Level != null
								? new LevelDto { Id = ts.Level.Id, Name = ts.Level.Name }
								: null
						})
						: new List<TutorSubjectDto>()))

				// mapping from UserProfile
				.ForMember(d => d.Gender, opt => opt.MapFrom(src =>
					src.UserEmailNavigation.UserProfile.Gender == null
						? null
						: (Gender?)src.UserEmailNavigation.UserProfile.Gender
				))

				.ForMember(d => d.Province, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.City))
				.ForMember(d => d.SubDistrict, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.SubDistrict))
				.ForMember(d => d.Dob, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.Dob))
				.ForMember(d => d.AvatarUrl, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.AvatarUrl))
				.ForMember(d => d.AddressLine, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.AddressLine))
				.ForMember(d => d.Latitude, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.Latitude))
				.ForMember(d => d.Longitude, opt => opt.MapFrom(src => src.UserEmailNavigation.UserProfile.Longitude))
				// mapping to User 
				.ForMember(d => d.UserName, opt => opt.MapFrom(src => src.UserEmailNavigation.UserName))
				.ForMember(d => d.Phone, opt => opt.MapFrom(src => src.UserEmailNavigation.Phone));


			//  Booking
			CreateMap<Booking, BookingDto>()
				.ForMember(dest => dest.PaymentStatus,
					opt => opt.MapFrom(src => (PaymentStatus)src.PaymentStatus))
				.ForMember(dest => dest.Status,
					opt => opt.MapFrom(src => (BookingStatus)src.Status))
				.ForMember(dest => dest.SystemFee, opt => opt.MapFrom(src => src.SystemFee != null ? src.SystemFee : null))
				.ForMember(dest => dest.TutorSubject,
					opt => opt.MapFrom(src => src.TutorSubject != null ? new TutorSubjectDto
					{
						Id = src.TutorSubject.Id,
						TutorId = src.TutorSubject.TutorId,
						TutorEmail = src.TutorSubject.Tutor.UserEmail ,
						HourlyRate = src.TutorSubject.HourlyRate,
						Subject = src.TutorSubject.Subject != null ? new SubjectDto
						{
							Id = src.TutorSubject.Subject.Id,
							SubjectName = src.TutorSubject.Subject.SubjectName
						} : null,
						Level = src.TutorSubject.Level != null ? new LevelDto
						{
							Id = src.TutorSubject.Level.Id,
							Name = src.TutorSubject.Level.Name
						} : null
					} : null))
				.ForMember(dest => dest.Schedules,
					opt => opt.MapFrom(src => src.Schedules != null ? src.Schedules.Select(s => new ScheduleDto
					{
						Id = s.Id,
						AvailabilitiId = s.AvailabilitiId,
						BookingId = s.BookingId,
						Status = (ScheduleStatus)s.Status,
						AttendanceNote = s.AttendanceNote,
						IsRefunded = s.IsRefunded,
						RefundedAt = s.RefundedAt,
						CreatedAt = s.CreatedAt,
						UpdatedAt = s.UpdatedAt,
						Availability = null,
						HasMeetingSession = s.MeetingSession != null,
						MeetingSession = null
					}) : null));
				

			//Schedule 
			CreateMap<Schedule, ScheduleDto>()
				.ForMember(dest => dest.Status,
					opt => opt.MapFrom(src => (ScheduleStatus)src.Status))
				.ForMember(dest => dest.Availability,
					opt => opt.MapFrom(src => src.Availabiliti != null ? new TutorAvailabilityDto
					{
						Id = src.Availabiliti.Id,
						TutorId = src.Availabiliti.TutorId,
						SlotId = src.Availabiliti.SlotId,
						StartDate = src.Availabiliti.StartDate,
						EndDate = src.Availabiliti.EndDate ?? (src.Availabiliti.Slot != null 
							? src.Availabiliti.StartDate.Date.Add(src.Availabiliti.Slot.EndTime.ToTimeSpan())
							: src.Availabiliti.StartDate),
						Status = (TutorAvailabilityStatus)src.Availabiliti.Status,
						CreatedAt = src.Availabiliti.CreatedAt,
						UpdatedAt = src.Availabiliti.UpdatedAt,
						Slot = src.Availabiliti.Slot != null ? new TimeSlotDto
						{
							Id = src.Availabiliti.Slot.Id,
							StartTime = src.Availabiliti.Slot.StartTime,
							EndTime = src.Availabiliti.Slot.EndTime
						} : null
					} : null))
				.ForMember(dest => dest.HasMeetingSession,
					opt => opt.MapFrom(src => src.MeetingSession != null))
				.ForMember(dest => dest.MeetingSession,
					opt => opt.MapFrom(src => src.MeetingSession != null ? new MeetingSessionDto
					{
						Id = src.MeetingSession.Id,
						MeetingType = (MeetingType)src.MeetingSession.MeetingType,
						StartTime = src.MeetingSession.StartTime,
						EndTime = src.MeetingSession.EndTime,
						MeetLink = src.MeetingSession.MeetLink,
						MeetCode = src.MeetingSession.MeetCode,
						CreatedAt = src.MeetingSession.CreatedAt,
					} : null))
				.ForMember(dest => dest.ScheduleCompletion,
					opt => opt.MapFrom(src => src.ScheduleCompletion != null ? new ScheduleCompletionDto
					{
						Id = src.ScheduleCompletion.Id,
						ScheduleId = src.ScheduleCompletion.ScheduleId,
						BookingId = src.ScheduleCompletion.BookingId,
						TutorId = src.ScheduleCompletion.TutorId,
						LearnerEmail = src.ScheduleCompletion.LearnerEmail,
						Status = (ScheduleCompletionStatus)src.ScheduleCompletion.Status,
						ConfirmationDeadline = src.ScheduleCompletion.ConfirmationDeadline,
						ConfirmedAt = src.ScheduleCompletion.ConfirmedAt,
						AutoCompletedAt = src.ScheduleCompletion.AutoCompletedAt,
						ReportId = src.ScheduleCompletion.ReportId,
						Note = src.ScheduleCompletion.Note,
						CreatedAt = src.ScheduleCompletion.CreatedAt,
						UpdatedAt = src.ScheduleCompletion.UpdatedAt
					} : null))
				.ForMember(dest => dest.TutorPayout,
					opt => opt.MapFrom(src => src.TutorPayout != null ? new TutorPayoutDto
					{
						Id = src.TutorPayout.Id,
						ScheduleId = src.TutorPayout.ScheduleId,
						BookingId = src.TutorPayout.BookingId,
						TutorWalletId = src.TutorPayout.TutorWalletId,
						Amount = src.TutorPayout.Amount,
						SystemFeeAmount = src.TutorPayout.SystemFeeAmount,
						Status = (TutorPayoutStatus)src.TutorPayout.Status,
						PayoutTrigger = src.TutorPayout.PayoutTrigger,
						ScheduledPayoutDate = src.TutorPayout.ScheduledPayoutDate,
						ReleasedAt = src.TutorPayout.ReleasedAt,
						WalletTransactionId = src.TutorPayout.WalletTransactionId,
						HoldReason = src.TutorPayout.HoldReason,
						CreatedAt = src.TutorPayout.CreatedAt,
						UpdatedAt = src.TutorPayout.UpdatedAt
					} : null));

            // MeetingSession 
            CreateMap<MeetingSession, MeetingSessionDto>()
                .ForMember(dest => dest.MeetingType,
                    opt => opt.MapFrom(src => (MeetingType)src.MeetingType));

            // GoogleToken 
            CreateMap<GoogleToken, GoogleTokenDto>();

            // SystemFee -> SystemFeeDto
            CreateMap<SystemFee, SystemFeeDto>();

            // RefundPolicy -> RefundPolicyDto
            CreateMap<RefundPolicy, RefundPolicyDto>();

            // BookingRefundRequest -> BookingRefundRequestDto
            CreateMap<BookingRefundRequest, BookingRefundRequestDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => (BookingRefundRequestStatus)src.Status))
                .ForMember(dest => dest.Booking,
                    opt => opt.MapFrom(src => src.Booking != null ? src.Booking : null))
                .ForMember(dest => dest.RefundPolicy,
                    opt => opt.MapFrom(src => src.RefundPolicy != null ? src.RefundPolicy : null))
                .ForMember(dest => dest.Learner,
                    opt => opt.MapFrom(src => src.LearnerEmailNavigation != null ? src.LearnerEmailNavigation : null))
                .ForMember(dest => dest.RefundRequestEvidences,
                    opt => opt.MapFrom(src => src.RefundRequestEvidences != null 
                        ? src.RefundRequestEvidences.Select(e => new RefundRequestEvidenceDto
                        {
                            Id = e.Id,
                            BookingRefundRequestId = e.BookingRefundRequestId,
                            FileUrl = e.FileUrl,
                            CreatedAt = e.CreatedAt,
                            BookingRefundRequest = null
                        }).ToList()
                        : null));

            // RefundRequestEvidence -> RefundRequestEvidenceDto
            CreateMap<RefundRequestEvidence, RefundRequestEvidenceDto>()
                .ForMember(dest => dest.BookingRefundRequest,
                    opt => opt.MapFrom(src => src.BookingRefundRequest != null ? src.BookingRefundRequest : null));

          

            // TutorVerificationRequest -> TutorVerificationRequestDto
            CreateMap<TutorVerificationRequest, TutorVerificationRequestDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => (TutorVerificationRequestStatus)src.Status))
                .ForMember(dest => dest.Tutor,
                    opt => opt.MapFrom(src => src.Tutor != null ? src.Tutor : null));







            // TutorSubject mappings
            CreateMap<TutorSubject, TutorSubjectDto>()
                .ForMember(dest => dest.TutorEmail, opt => opt.MapFrom(src => src.Tutor.UserEmail));






			// User mappings
			CreateMap<User, UserDto>().ReverseMap();
			CreateMap<UserUpdateRequest, User>()
				.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
		


			// Role mappings
			CreateMap<Role, RoleDto>().ReverseMap();

			// RefreshToken mappings
			CreateMap<RefreshToken, RefreshTokenDto>().ReverseMap();

			// UserProfile mappings
			CreateMap<UserProfile, UserProfileDto>().ReverseMap();

			CreateMap<UserProfileUpdateRequest, UserProfile>()
			.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


			// Province mappings
			CreateMap<Province, ProvinceDto>();

			// SubDistrict mappings
			CreateMap<SubDistrict, SubDistrictDto>();


            CreateMap<Notification, NotificationDto>();

            // ScheduleChangeRequest mappings
            CreateMap<ScheduleChangeRequest, ScheduleChangeRequestDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => (ScheduleChangeRequestStatus)src.Status))
                .ForMember(dest => dest.Schedule,
                    opt => opt.MapFrom(src => src.Schedule != null ? new ScheduleDto
                    {
                        Id = src.Schedule.Id,
                        AvailabilitiId = src.Schedule.AvailabilitiId,
                        BookingId = src.Schedule.BookingId,
                        Status = (ScheduleStatus)src.Schedule.Status,
                        AttendanceNote = src.Schedule.AttendanceNote,
                        IsRefunded = src.Schedule.IsRefunded,
                        RefundedAt = src.Schedule.RefundedAt,
                        CreatedAt = src.Schedule.CreatedAt,
                        UpdatedAt = src.Schedule.UpdatedAt
                    } : null))
                .ForMember(dest => dest.OldAvailability,
                    opt => opt.MapFrom(src => src.OldAvailabiliti != null ? new TutorAvailabilityDto
                    {
                        Id = src.OldAvailabiliti.Id,
                        TutorId = src.OldAvailabiliti.TutorId,
                        SlotId = src.OldAvailabiliti.SlotId,
                        StartDate = src.OldAvailabiliti.StartDate,
                        EndDate = src.OldAvailabiliti.EndDate,
                        Status = (TutorAvailabilityStatus)src.OldAvailabiliti.Status,
                        CreatedAt = src.OldAvailabiliti.CreatedAt,
                        UpdatedAt = src.OldAvailabiliti.UpdatedAt,
                        Slot = src.OldAvailabiliti.Slot != null ? new TimeSlotDto
                        {
                            Id = src.OldAvailabiliti.Slot.Id,
                            StartTime = src.OldAvailabiliti.Slot.StartTime,
                            EndTime = src.OldAvailabiliti.Slot.EndTime
                        } : null
                    } : null))
                .ForMember(dest => dest.NewAvailability,
                    opt => opt.MapFrom(src => src.NewAvailabiliti != null ? new TutorAvailabilityDto
                    {
                        Id = src.NewAvailabiliti.Id,
                        TutorId = src.NewAvailabiliti.TutorId,
                        SlotId = src.NewAvailabiliti.SlotId,
                        StartDate = src.NewAvailabiliti.StartDate,
                        EndDate = src.NewAvailabiliti.EndDate,
                        Status = (TutorAvailabilityStatus)src.NewAvailabiliti.Status,
                        CreatedAt = src.NewAvailabiliti.CreatedAt,
                        UpdatedAt = src.NewAvailabiliti.UpdatedAt,
                        Slot = src.NewAvailabiliti.Slot != null ? new TimeSlotDto
                        {
                            Id = src.NewAvailabiliti.Slot.Id,
                            StartTime = src.NewAvailabiliti.Slot.StartTime,
                            EndTime = src.NewAvailabiliti.Slot.EndTime
                        } : null
                    } : null));

            // Report mappings
            CreateMap<Report, ReportListItemDto>()
                .ForMember(dest => dest.ReporterEmail, opt => opt.MapFrom(src => src.ReporterUserEmail))
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.ReporterUserEmailNavigation != null ? src.ReporterUserEmailNavigation.UserName : null))
                .ForMember(dest => dest.ReporterAvatarUrl, opt => opt.MapFrom(src => src.ReporterUserEmailNavigation != null && src.ReporterUserEmailNavigation.UserProfile != null ? src.ReporterUserEmailNavigation.UserProfile.AvatarUrl : null))
                .ForMember(dest => dest.ReportedUserName, opt => opt.MapFrom(src => src.ReportedUserEmailNavigation != null ? src.ReportedUserEmailNavigation.UserName : null))
                .ForMember(dest => dest.ReportedAvatarUrl, opt => opt.MapFrom(src => src.ReportedUserEmailNavigation != null && src.ReportedUserEmailNavigation.UserProfile != null ? src.ReportedUserEmailNavigation.UserProfile.AvatarUrl : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (ReportStatus)src.Status));

            CreateMap<Report, ReportDetailDto>()
                .IncludeBase<Report, ReportListItemDto>()
                .ForMember(dest => dest.Booking, opt => opt.MapFrom(src => src.Booking));

            CreateMap<ReportEvidence, ReportEvidenceDto>()
                .ForMember(dest => dest.MediaType, opt => opt.MapFrom(src => (MediaType)src.MediaType))
                .ForMember(dest => dest.EvidenceType, opt => opt.MapFrom(src => (ReportEvidenceType)src.EvidenceType));

            CreateMap<ReportDefense, ReportDefenseDto>();

            CreateMap<BookingNote, BookingNoteDto>()
                .ForMember(dest => dest.Media,
                    opt => opt.MapFrom(src => src.BookingNoteMedia != null ? src.BookingNoteMedia : new List<BookingNoteMedium>()));
            CreateMap<BookingNoteMedium, BookingNoteMediaDto>()
                .ForMember(dest => dest.MediaType, opt => opt.MapFrom(src => (MediaType)src.MediaType));

        }
    }
}
