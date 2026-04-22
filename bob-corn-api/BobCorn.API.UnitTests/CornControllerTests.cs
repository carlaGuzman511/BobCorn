using BobCorn.API.Controllers;
using BobCorn.Application.Abstractions.Persistence;
using BobCorn.Application.Abstractions.Time;
using BobCorn.Application.UseCases.PurchaseCorn;
]using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BobCorn.API.UnitTests
{
    public class CornControllerTests
    {
        private readonly Mock<IDbConnectionFactory> _dbConnectionFactory;
        private readonly Mock<IClock> _clockMock;
        private readonly Mock<ICornPurchaseRepository> _cornPurchaseRepositoryMock;
        private readonly Mock<PurchaseCornHandler> _handlerMock;
        private readonly CornController _controller;

        public CornControllerTests()
        {
            _dbConnectionFactory = new Mock<IDbConnectionFactory>();
            _clockMock = new Mock<IClock>();
            _cornPurchaseRepositoryMock = new Mock<ICornPurchaseRepository>(_dbConnectionFactory.Object);
            _handlerMock = new Mock<PurchaseCornHandler>(_cornPurchaseRepositoryMock.Object, _clockMock.Object);

            _controller = new CornController(_handlerMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task Purchase_ShouldReturn200_WhenSuccess()
        {
            var clientId = "client-0001";
            var nextAllowedAt = DateTimeOffset.UtcNow.AddMinutes(1);

            _controller.HttpContext.Request.Headers["X-Client-Id"] = clientId;

            _handlerMock
                .Setup(x => x.Handle(clientId))
                .ReturnsAsync(PurchaseResult.Success(nextAllowedAt));

            var result = await _controller.Purchase();

            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            dynamic body = okResult.Value!;
            ((int)body.totalPurchased).Should().Be(5);
        }

        [Fact]
        public async Task Purchase_ShouldReturn429_WhenRateLimited()
        {
            var clientId = "client-0001";
            var retryAt = DateTimeOffset.UtcNow.AddSeconds(30);

            _controller.HttpContext.Request.Headers["X-Client-Id"] = clientId;

            _handlerMock
                .Setup(x => x.Handle(clientId))
                .ReturnsAsync(PurchaseResult.Failure(retryAt));

            var result = await _controller.Purchase();

            var objectResult = result as ObjectResult;

            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(429);

            var retryHeader = _controller.Response.Headers["Retry-After"];
            retryHeader.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Purchase_ShouldSetCorrectRetryAfterHeader()
        {
            var clientId = "client-0001";
            var now = DateTimeOffset.UtcNow;
            var retryAt = now.AddSeconds(45);

            _controller.HttpContext.Request.Headers["X-Client-Id"] = clientId;

            _handlerMock
                .Setup(x => x.Handle(clientId))
                .ReturnsAsync(PurchaseResult.Failure(retryAt));

            await _controller.Purchase();

            var headerValue = _controller.Response.Headers["Retry-After"].ToString();

            int seconds = int.Parse(headerValue);
            seconds.Should().BeInRange(40, 45);
        }


    }
}