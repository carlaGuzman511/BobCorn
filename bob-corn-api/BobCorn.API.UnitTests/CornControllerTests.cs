using BobCorn.API.Controllers;
using BobCorn.API.Models;
using BobCorn.Application.Abstractions.Persistence;
using BobCorn.Application.UseCases.PurchaseCorn;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BobCorn.API.UnitTests
{
    public class CornControllerTests
    {
        private readonly Mock<ICornPurchaseRepository> _repositoryMock;
        private readonly PurchaseCornHandler _handler;
        private readonly CornController _controller;

        public CornControllerTests()
        {
            _repositoryMock = new Mock<ICornPurchaseRepository>();
            _handler = new PurchaseCornHandler(_repositoryMock.Object);
            _controller = new CornController(_handler);

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
            var totalPurchased = 5;

            _controller.HttpContext.Request.Headers["X-Client-Id"] = clientId;

            _repositoryMock
               .Setup(r => r.GetTotalPurchasesAsync(clientId))
               .ReturnsAsync(totalPurchased);

            _repositoryMock
                .Setup(r => r.TryPurchaseAsync(clientId))
                .ReturnsAsync((true, nextAllowedAt));

            var result = await _controller.Purchase();

            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as PurchaseResponse;
            response.TotalPurchased.Should().Be(totalPurchased);
            response.NextAllowedAt.Should().Be(nextAllowedAt);
        }

        [Fact]
        public async Task Purchase_ShouldReturn429_WhenRateLimited()
        {
            var clientId = "client-0001";
            var retryAt = DateTimeOffset.UtcNow.AddSeconds(30);
            var totalPurchased = 5;

            _repositoryMock
                .Setup(r => r.GetTotalPurchasesAsync(clientId))
                .ReturnsAsync(totalPurchased);

            _repositoryMock
                .Setup(r => r.TryPurchaseAsync(clientId))
                .ReturnsAsync((false, retryAt));

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
            var totalPurchased = 5;

            _controller.HttpContext.Request.Headers["X-Client-Id"] = clientId;

            _repositoryMock
                .Setup(r => r.GetTotalPurchasesAsync(clientId))
                .ReturnsAsync(totalPurchased);

            _repositoryMock
                .Setup(r => r.TryPurchaseAsync(clientId))
                .ReturnsAsync((false, retryAt));

            await _controller.Purchase();

            var headerValue = _controller.Response.Headers["Retry-After"].ToString();

            int seconds = int.Parse(headerValue);
            seconds.Should().BeInRange(40, 45);
        }


    }
}