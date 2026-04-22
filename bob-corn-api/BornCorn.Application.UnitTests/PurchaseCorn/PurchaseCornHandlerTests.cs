using Xunit;
using Moq;
using FluentAssertions;
using BobCorn.Application.Abstractions.Persistence;
using BobCorn.Application.UseCases.PurchaseCorn;
using BobCorn.Application.Abstractions.Time;

namespace BornCorn.Application.UnitTests.PurchaseCorn
{
    public class PurchaseCornHandlerTests
    {
        private readonly Mock<ICornPurchaseRepository> _repositoryMock;
        private readonly Mock<IClock> _clockMock;
        private readonly PurchaseCornHandler _handler;

        public PurchaseCornHandlerTests()
        {
            _repositoryMock = new Mock<ICornPurchaseRepository>();
            _clockMock = new Mock<IClock>();

            _handler = new PurchaseCornHandler(
                _repositoryMock.Object,
                _clockMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldAllowPurchase_WhenNoPreviousPurchase()
        {
            var clientId = "client-0001";
            var now = DateTimeOffset.UtcNow;

            _clockMock.Setup(x => x.UtcNow).Returns(now);
            _repositoryMock
                .Setup(x => x.GetLastPurchaseAsync(clientId))
                .ReturnsAsync((DateTimeOffset?)null);

            var result = await _handler.Handle(clientId);

            result.IsSuccess.Should().BeTrue();
            result.NextAllowedAt.Should().Be(now.AddMinutes(1));

            _repositoryMock.Verify(x =>
                x.SetLastPurchaseAsync(clientId, now), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldBlockPurchase_WhenWithinOneMinute()
        {
            var clientId = "client-0001";
            var now = DateTimeOffset.UtcNow;
            var lastPurchase = now.AddSeconds(-30);

            _clockMock.Setup(x => x.UtcNow).Returns(now);
            _repositoryMock
                .Setup(x => x.GetLastPurchaseAsync(clientId))
                .ReturnsAsync(lastPurchase);

            var result = await _handler.Handle(clientId);

            result.IsSuccess.Should().BeFalse();
            result.NextAllowedAt.Should().Be(lastPurchase.AddMinutes(1));

            _repositoryMock.Verify(x =>
                x.SetLastPurchaseAsync(It.IsAny<string>(), It.IsAny<DateTimeOffset>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldAllowPurchase_WhenMoreThanOneMinutePassed()
        {
            var clientId = "client-0001";
            var now = DateTimeOffset.UtcNow;
            var lastPurchase = now.AddMinutes(-2);

            _clockMock.Setup(x => x.UtcNow).Returns(now);
            _repositoryMock
                .Setup(x => x.GetLastPurchaseAsync(clientId))
                .ReturnsAsync(lastPurchase);

            var result = await _handler.Handle(clientId);

            result.IsSuccess.Should().BeTrue();
            result.NextAllowedAt.Should().Be(now.AddMinutes(1));

            _repositoryMock.Verify(x =>
                x.SetLastPurchaseAsync(clientId, now), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldAllowPurchase_WhenExactlyOneMinutePassed()
        {
            var clientId = "client-0001";
            var now = DateTimeOffset.UtcNow;
            var lastPurchase = now.AddSeconds(-60);

            _clockMock.Setup(x => x.UtcNow).Returns(now);
            _repositoryMock
                .Setup(x => x.GetLastPurchaseAsync(clientId))
                .ReturnsAsync(lastPurchase);

            var result = await _handler.Handle(clientId);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldThrow_WhenRepositoryFails()
        {
            var clientId = "client-0001";
            var now = DateTimeOffset.UtcNow;

            _clockMock.Setup(x => x.UtcNow).Returns(now);

            _repositoryMock
                .Setup(x => x.GetLastPurchaseAsync(clientId))
                .ThrowsAsync(new Exception("DB failure"));

            Func<Task> act = async () => await _handler.Handle(clientId);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("DB failure");
        }

        [Theory]
        [InlineData(-30, false)]
        [InlineData(-59, false)]
        [InlineData(-60, true)]
        [InlineData(-120, true)]
        public async Task Handle_ShouldRespectRateLimit(int secondsOffset, bool expectedSuccess)
        {
            var clientId = "client-0001";
            var now = DateTimeOffset.UtcNow;
            var lastPurchase = now.AddSeconds(secondsOffset);

            _clockMock.Setup(x => x.UtcNow).Returns(now);
            _repositoryMock
                .Setup(x => x.GetLastPurchaseAsync(clientId))
                .ReturnsAsync(lastPurchase);

            var result = await _handler.Handle(clientId);

            result.IsSuccess.Should().Be(expectedSuccess);
        }
    }
}