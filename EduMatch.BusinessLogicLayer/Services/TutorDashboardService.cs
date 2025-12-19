using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class TutorDashboardService : ITutorDashboardService
    {
        private readonly EduMatchContext _context;
        private readonly IMapper _mapper;
        private readonly TimeZoneInfo _vietnamTz;

        public TutorDashboardService(EduMatchContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _vietnamTz = ResolveVietnamTimeZone();
        }

        public async Task<IReadOnlyList<ScheduleDto>> GetUpcomingLessonsAsync(string tutorEmail)
        {
            var email = NormalizeEmail(tutorEmail);
            var nowVn = GetVietnamNow();
            var startDate = nowVn.Date;
            var endDate = startDate.AddDays(2);

            var schedules = await BuildTutorScheduleQuery(email)
                .Where(s => s.Availabiliti.StartDate.Date >= startDate && s.Availabiliti.StartDate.Date <= endDate)
                .Where(s => s.Status == (int)ScheduleStatus.Upcoming)
                .OrderBy(s => s.Availabiliti.StartDate)
                .ThenBy(s => s.Id)
                .ToListAsync();

            return _mapper.Map<List<ScheduleDto>>(schedules);
        }

        public async Task<IReadOnlyList<ScheduleDto>> GetTodaySchedulesAsync(string tutorEmail)
        {
            var email = NormalizeEmail(tutorEmail);
            var today = GetVietnamNow().Date;

            var schedules = await BuildTutorScheduleQuery(email)
                .Where(s => s.Availabiliti.StartDate.Date == today)
                .Where(s => s.Status != (int)ScheduleStatus.Cancelled)
                .OrderBy(s => s.Availabiliti.StartDate)
                .ThenBy(s => s.Id)
                .ToListAsync();

            return _mapper.Map<List<ScheduleDto>>(schedules);
        }

        public async Task<IReadOnlyList<BookingDto>> GetPendingBookingsAsync(string tutorEmail)
        {
            var email = NormalizeEmail(tutorEmail);

            var bookings = await _context.Bookings
                .AsNoTracking()
                .AsSplitQuery()
                .Include(b => b.SystemFee)
                .Include(b => b.Schedules)
                    .ThenInclude(s => s.MeetingSession)
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Tutor)
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Subject)
                .Include(b => b.TutorSubject)
                    .ThenInclude(ts => ts.Level)
                .Where(b => b.Status == (int)BookingStatus.Pending)
                .Where(b => b.PaymentStatus == (int)PaymentStatus.Paid)
                .Where(b => b.TutorSubject.Tutor.UserEmail == email)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<BookingDto>>(bookings);
        }

        public async Task<IReadOnlyList<TutorMonthlyEarningDto>> GetMonthlyEarningsAsync(string tutorEmail, int year)
        {
            if (year <= 0)
                throw new ArgumentException("Year must be a positive integer.", nameof(year));

            var email = NormalizeEmail(tutorEmail);
            var start = new DateTime(year, 1, 1, 0, 0, 0);
            var end = start.AddYears(1);

            var payouts = await _context.TutorPayouts
                .AsNoTracking()
                .Include(p => p.TutorWallet)
                .Where(p => p.TutorWallet.UserEmail == email)
                .Where(p => p.Status == (byte)TutorPayoutStatus.Paid)
                .Where(p =>
                    (p.ReleasedAt.HasValue && p.ReleasedAt.Value >= start && p.ReleasedAt.Value < end) ||
                    (!p.ReleasedAt.HasValue && p.CreatedAt >= start && p.CreatedAt < end))
                .ToListAsync();

            var results = new List<TutorMonthlyEarningDto>(12);
            for (var month = 1; month <= 12; month++)
            {
                var monthStart = new DateTime(year, month, 1, 0, 0, 0);
                var monthEnd = monthStart.AddMonths(1);

                var amount = payouts
                    .Where(p =>
                    {
                        var date = p.ReleasedAt ?? p.CreatedAt;
                        return date >= monthStart && date < monthEnd;
                    })
                    .Sum(p => p.Amount);

                results.Add(new TutorMonthlyEarningDto
                {
                    Year = year,
                    Month = month,
                    Earning = amount
                });
            }

            return results;
        }

        public async Task<TutorMonthlyEarningDto> GetCurrentMonthEarningAsync(string tutorEmail)
        {
            var email = NormalizeEmail(tutorEmail);
            var nowVn = GetVietnamNow();
            var monthStart = new DateTime(nowVn.Year, nowVn.Month, 1, 0, 0, 0);
            var monthEnd = monthStart.AddMonths(1);

            var payouts = await _context.TutorPayouts
                .AsNoTracking()
                .Include(p => p.TutorWallet)
                .Where(p => p.TutorWallet.UserEmail == email)
                .Where(p => p.Status == (byte)TutorPayoutStatus.Paid)
                .Where(p =>
                    (p.ReleasedAt.HasValue && p.ReleasedAt.Value >= monthStart && p.ReleasedAt.Value < monthEnd) ||
                    (!p.ReleasedAt.HasValue && p.CreatedAt >= monthStart && p.CreatedAt < monthEnd))
                .ToListAsync();

            var total = payouts.Sum(p => p.Amount);

            return new TutorMonthlyEarningDto
            {
                Year = nowVn.Year,
                Month = nowVn.Month,
                Earning = total
            };
        }

        public async Task<IReadOnlyList<ReportListItemDto>> GetReportsPendingDefenseAsync(string tutorEmail)
        {
            var email = NormalizeEmail(tutorEmail);

            var reports = await _context.Reports
                .AsNoTracking()
                .Include(r => r.ReporterUserEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                .Include(r => r.ReportedUserEmailNavigation)
                    .ThenInclude(u => u.UserProfile)
                .Where(r => r.ReportedUserEmail == email)
                .Where(r => r.Status == (int)ReportStatus.Pending || r.Status == (int)ReportStatus.UnderReview)
                .Where(r => !_context.ReportDefenses.Any(d => d.ReportId == r.Id && d.TutorEmail == email))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<ReportListItemDto>>(reports);
        }

        private IQueryable<Schedule> BuildTutorScheduleQuery(string tutorEmail)
        {
            return _context.Schedules
                .AsNoTracking()
                .AsSplitQuery()
                .Include(s => s.Availabiliti)
                    .ThenInclude(a => a.Slot)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Tutor)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Subject)
                .Include(s => s.Booking)
                    .ThenInclude(b => b.TutorSubject)
                        .ThenInclude(ts => ts.Level)
                .Include(s => s.MeetingSession)
                .Include(s => s.ScheduleCompletion)
                .Include(s => s.TutorPayout)
                .Where(s => s.Booking.TutorSubject.Tutor.UserEmail == tutorEmail);
        }

        private DateTime GetVietnamNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTz);
        }

        private static string NormalizeEmail(string tutorEmail)
        {
            if (string.IsNullOrWhiteSpace(tutorEmail))
                throw new ArgumentException("Tutor email is required.", nameof(tutorEmail));

            return tutorEmail.Trim();
        }

        private static TimeZoneInfo ResolveVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch (InvalidTimeZoneException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
        }
    }
}
