using Moq;
using FluentAssertions;
using BobCorn.Application.Abstractions.Persistence;
using BobCorn.Application.UseCases.PurchaseCorn;

namespace BornCorn.Application.UnitTests.PurchaseCorn
{
    public class PurchaseCornHandlerTests
    {
        private readonly Mock<ICornPurchaseRepository> _repositoryMock;
        private readonly PurchaseCornHandler _handler;

        public PurchaseCornHandlerTests()
        {
            _repositoryMock = new Mock<ICornPurchaseRepository>();

            _handler = new PurchaseCornHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenPurchaseAllowed()
        {
            var clientId = "client-0001";
            var next = DateTimeOffset.UtcNow.AddMinutes(1);

            _repositoryMock
                .Setup(x => x.TryPurchaseAsync(clientId))
                .ReturnsAsync((true, next));

            var result = await _handler.Handle(clientId);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRateLimited()
        {
            var clientId = "client-0001";
            var next = DateTimeOffset.UtcNow.AddSeconds(30);

            _repositoryMock
                .Setup(x => x.TryPurchaseAsync(clientId))
                .ReturnsAsync((false, next));

            var result = await _handler.Handle(clientId);

            result.IsSuccess.Should().BeFalse();
        }


        [Fact]
        public async Task Handle_ShouldAllowPurchase_WhenExactlyOneMinutePassed()
        {
            var clientId = "client-0001";
            var next = DateTimeOffset.UtcNow.AddMinutes(1);

            _repositoryMock
                .Setup(x => x.TryPurchaseAsync(clientId))
                .ReturnsAsync((true, next));

            var result = await _handler.Handle(clientId);

            result.IsSuccess.Should().BeTrue();

        }


        [Theory]
        [InlineData(30, false)]
        [InlineData(59, false)]
        [InlineData(60, true)]
        [InlineData(120, true)]
        public async Task Handle_ShouldRespectRateLimit(int secondsOffset, bool expectedSuccess)
        {
            var clientId = "client-0001";
            var next = DateTimeOffset.UtcNow.AddSeconds(secondsOffset);

            _repositoryMock
                .Setup(x => x.TryPurchaseAsync(clientId))
                .ReturnsAsync((expectedSuccess, next));

            var result = await _handler.Handle(clientId);

            result.IsSuccess.Should().Be(expectedSuccess);

        }
    }
}