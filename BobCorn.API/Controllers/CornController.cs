using BobCorn.Application.Abstractions.Persistence;
using BobCorn.Application.UseCases.PurchaseCorn;
using Microsoft.AspNetCore.Mvc;

namespace BobCorn.API.Controllers
{
    [ApiController]
    [Route("api/corn")]
    public class CornController : ControllerBase
    {
        private readonly PurchaseCornHandler _handler;

        public CornController(PurchaseCornHandler handler)
        {
            _handler = handler;
        }

        [HttpPost("purchase")]
        public async Task<IActionResult> Purchase()
        {
            var clientId = GetClientId();

            var result = await _handler.Handle(clientId);

            if (!result.IsSuccess)
            {
                Response.Headers["Retry-After"] =
                    ((int)(result.NextAllowedAt.Value - DateTimeOffset.UtcNow).TotalSeconds).ToString();

                return StatusCode(429, new
                {
                    message = "Too many requests",
                    nextAllowedAt = result.NextAllowedAt
                });
            }

            return Ok(new
            {
                message = "Corn purchased",
                nextAllowedAt = result.NextAllowedAt
            });
        }

        private string GetClientId()
        {
            return Request.Headers["X-Client-Id"].FirstOrDefault()
                   ?? HttpContext.Connection.RemoteIpAddress?.ToString()
                   ?? Guid.NewGuid().ToString();
        }
    }

}
