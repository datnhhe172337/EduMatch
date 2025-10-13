using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using System;

namespace EduMatch.BusinessLogicLayer.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			// CertificateType mappings
			CreateMap<CertificateType, CertificateTypeDto>().ReverseMap();
			CreateMap<CertificateTypeCreateRequest, CertificateType>()
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));
			CreateMap<CertificateTypeUpdateRequest, CertificateType>();

			// CertificateTypeSubject mappings
			CreateMap<CertificateTypeSubject, CertificateTypeSubjectDto>().ReverseMap();

			// EducationInstitution mappings
			CreateMap<EducationInstitution, EducationInstitutionDto>().ReverseMap();

			// EducationInstitutionLevel mappings
			CreateMap<EducationInstitutionLevel, EducationInstitutionLevelDto>().ReverseMap();

			// EducationLevel mappings
			CreateMap<EducationLevel, EducationLevelDto>().ReverseMap();

			// Level mappings
			CreateMap<Level, LevelDto>().ReverseMap();
			CreateMap<LevelCreateRequest, Level>();
			CreateMap<LevelUpdateRequest, Level>();

			// Subject mappings
			CreateMap<Subject, SubjectDto>().ReverseMap();
			CreateMap<SubjectCreateRequest, Subject>();
			CreateMap<SubjectUpdateRequest, Subject>();

			// TimeSlot mappings
			CreateMap<TimeSlot, TimeSlotDto>().ReverseMap();
			CreateMap<TimeSlotCreateRequest, TimeSlot>();
			CreateMap<TimeSlotUpdateRequest, TimeSlot>();

			// TutorAvailability mappings
			CreateMap<TutorAvailability, TutorAvailabilityDto>().ReverseMap();
			CreateMap<TutorAvailabilityCreateRequest, TutorAvailability>();
			CreateMap<TutorAvailabilityUpdateRequest, TutorAvailability>();

			// TutorCertificate mappings
			CreateMap<TutorCertificate, TutorCertificateDto>().ReverseMap();
			CreateMap<TutorCertificateCreateRequest, TutorCertificate>()
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
				.ForMember(dest => dest.Verified, opt => opt.MapFrom(src => VerifyStatus.Pending));
			CreateMap<TutorCertificateUpdateRequest, TutorCertificate>();

			// TutorEducation mappings
			CreateMap<TutorEducation, TutorEducationDto>().ReverseMap();
			CreateMap<TutorEducationCreateRequest, TutorEducation>()
				.ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
				.ForMember(dest => dest.Verified, opt => opt.MapFrom(src => VerifyStatus.Pending));
			CreateMap<TutorEducationUpdateRequest, TutorEducation>();

			// TutorSubject mappings
			CreateMap<TutorSubject, TutorSubjectDto>().ReverseMap();
			CreateMap<TutorSubjectCreateRequest, TutorSubject>();
			CreateMap<TutorSubjectUpdateRequest, TutorSubject>();

			// TutorProfile mappings
			CreateMap<TutorProfile, TutorProfileDto>().ReverseMap();

			// User mappings
			CreateMap<User, UserDto>().ReverseMap();

			// Role mappings
			CreateMap<Role, RoleDto>().ReverseMap();

			// RefreshToken mappings
			CreateMap<RefreshToken, RefreshTokenDto>().ReverseMap();

			// UserProfile mappings
			CreateMap<UserProfile, UserProfileDto>().ReverseMap();

			// Province mappings
			CreateMap<Province, ProvinceDto>().ReverseMap();

			// SubDistrict mappings
			CreateMap<SubDistrict, SubDistrictDto>().ReverseMap();
		}
	}
}
