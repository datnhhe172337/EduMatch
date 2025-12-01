using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.BookingNote;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Moq;
using Xunit;

namespace EduMatch.UnitTests
{
    #region BookingNoteServiceTests
    public class BookingNoteServiceTests
    {
        private readonly Mock<IBookingNoteRepository> _noteRepo = new();
        private readonly Mock<IBookingRepository> _bookingRepo = new();
        private readonly IMapper _mapper;

        public BookingNoteServiceTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = new Mapper(config);
        }

        private BookingNoteService CreateService() =>
            new BookingNoteService(_noteRepo.Object, _bookingRepo.Object, _mapper);

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_WhenBookingNotFound_Throws()
        {
            _bookingRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Booking?)null);
            var service = CreateService();

            await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(new BookingNoteCreateRequest
            {
                BookingId = 99,
                Content = "note"
            }));
        }

        [Fact]
        public async Task CreateAsync_PersistsAndReturnsDto()
        {
            var booking = new Booking { Id = 1 };
            _bookingRepo.Setup(r => r.GetByIdAsync(booking.Id)).ReturnsAsync(booking);
            _noteRepo.Setup(r => r.CreateAsync(It.IsAny<BookingNote>()))
                .ReturnsAsync((BookingNote n) =>
                {
                    n.Id = 5;
                    return n;
                });

            var service = CreateService();
            var result = await service.CreateAsync(new BookingNoteCreateRequest
            {
                BookingId = booking.Id,
                Content = "New note",
                ImageUrl = "https://img",
                VideoUrl = null
            });

            _noteRepo.Verify(r => r.CreateAsync(It.IsAny<BookingNote>()), Times.Once);
            Assert.Equal(5, result.Id);
            Assert.Equal(booking.Id, result.BookingId);
            Assert.Equal("New note", result.Content);
            Assert.Equal("https://img", result.ImageUrl);
        }

        [Fact]
        public async Task CreateAsync_WithWhitespaceContent_Throws()
        {
            var service = CreateService();
            await Assert.ThrowsAsync<Exception>(() => service.CreateAsync(new BookingNoteCreateRequest
            {
                BookingId = 1,
                Content = "   "
            }));
        }

        [Fact]
        public async Task CreateAsync_TrimsAndNullsOptionalMedia()
        {
            var booking = new Booking { Id = 2 };
            _bookingRepo.Setup(r => r.GetByIdAsync(booking.Id)).ReturnsAsync(booking);
            _noteRepo.Setup(r => r.CreateAsync(It.IsAny<BookingNote>()))
                .ReturnsAsync((BookingNote n) =>
                {
                    n.Id = 9;
                    return n;
                });

            var service = CreateService();
            var result = await service.CreateAsync(new BookingNoteCreateRequest
            {
                BookingId = booking.Id,
                Content = "  spaced  ",
                ImageUrl = "  ",
                VideoUrl = ""
            });

            Assert.Equal("spaced", result.Content);
            Assert.Null(result.ImageUrl);
            Assert.Null(result.VideoUrl);
        }
        #endregion

        #region GetByIdAsync
        [Fact]
        public async Task GetByIdAsync_ReturnsNote()
        {
            var note = new BookingNote { Id = 7, BookingId = 1, Content = "hi" };
            _noteRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(note);
            var service = CreateService();

            var result = await service.GetByIdAsync(7);

            Assert.NotNull(result);
            Assert.Equal(7, result!.Id);
            Assert.Equal("hi", result.Content);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_Throws()
        {
            var service = CreateService();
            await Assert.ThrowsAsync<Exception>(() => service.GetByIdAsync(0));
        }
        #endregion

        #region GetByBookingIdAsync
        [Fact]
        public async Task GetByBookingIdAsync_ReturnsNotes()
        {
            var notes = new List<BookingNote>
            {
                new BookingNote { Id = 1, BookingId = 3, Content = "a" },
                new BookingNote { Id = 2, BookingId = 3, Content = "b" }
            };
            _noteRepo.Setup(r => r.GetByBookingIdAsync(3)).ReturnsAsync(notes);
            var service = CreateService();

            var result = await service.GetByBookingIdAsync(3);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, n => n.Content == "a");
            Assert.Contains(result, n => n.Content == "b");
        }

        [Fact]
        public async Task GetByBookingIdAsync_WithInvalidId_Throws()
        {
            var service = CreateService();
            await Assert.ThrowsAsync<Exception>(() => service.GetByBookingIdAsync(0));
        }
        #endregion

        #region UpdateAsync
        [Fact]
        public async Task UpdateAsync_WhenNotFound_ReturnsNull()
        {
            _noteRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((BookingNote?)null);
            var service = CreateService();

            var result = await service.UpdateAsync(new BookingNoteUpdateRequest
            {
                Id = 10,
                Content = "updated"
            });

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesFields()
        {
            var existing = new BookingNote
            {
                Id = 2,
                BookingId = 1,
                Content = "old",
                ImageUrl = null,
                VideoUrl = null
            };

            _noteRepo.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
            _noteRepo.Setup(r => r.UpdateAsync(It.IsAny<BookingNote>()))
                .ReturnsAsync((BookingNote n) => n);

            var service = CreateService();
            var result = await service.UpdateAsync(new BookingNoteUpdateRequest
            {
                Id = existing.Id,
                Content = "new content",
                ImageUrl = "img",
                VideoUrl = "vid"
            });

            _noteRepo.Verify(r => r.UpdateAsync(It.Is<BookingNote>(n =>
                n.Id == existing.Id &&
                n.Content == "new content" &&
                n.ImageUrl == "img" &&
                n.VideoUrl == "vid")), Times.Once);

            Assert.NotNull(result);
            Assert.Equal("new content", result!.Content);
            Assert.Equal("img", result.ImageUrl);
            Assert.Equal("vid", result.VideoUrl);
        }

        [Fact]
        public async Task UpdateAsync_WhenRepoReturnsNullAfterUpdate_ReturnsNull()
        {
            var existing = new BookingNote { Id = 11, BookingId = 1, Content = "old" };
            _noteRepo.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
            _noteRepo.Setup(r => r.UpdateAsync(It.IsAny<BookingNote>()))
                .ReturnsAsync((BookingNote?)null);

            var service = CreateService();
            var result = await service.UpdateAsync(new BookingNoteUpdateRequest
            {
                Id = existing.Id,
                Content = "new"
            });

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidId_Throws()
        {
            var service = CreateService();
            await Assert.ThrowsAsync<Exception>(() => service.UpdateAsync(new BookingNoteUpdateRequest
            {
                Id = 0,
                Content = "c"
            }));
        }

        [Fact]
        public async Task UpdateAsync_WithWhitespaceContent_Throws()
        {
            var service = CreateService();
            await Assert.ThrowsAsync<Exception>(() => service.UpdateAsync(new BookingNoteUpdateRequest
            {
                Id = 1,
                Content = "   "
            }));
        }
        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_ReturnsRepoResult()
        {
            _noteRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);
            _noteRepo.Setup(r => r.DeleteAsync(2)).ReturnsAsync(false);
            var service = CreateService();

            Assert.True(await service.DeleteAsync(1));
            Assert.False(await service.DeleteAsync(2));
        }
        #endregion
    }
    #endregion
}
