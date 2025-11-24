using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SlurkExp.Services.Hub
{
    [Route("api/hubs")]
    public class HubController : ControllerBase
    {
        private readonly IHubService _hubService;
        private readonly ILogger<HubController> _logger;

        public HubController(
            IHubService hubService,
            ILogger<HubController> logger)
        {
            _hubService = hubService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("agent-hub-test")]
        public async Task<IActionResult> SignaAgentHub()
        {
            await _hubService.SignalAgentHub("Hello to AgentHub!");
            return new JsonResult("ok");
        }

        [AllowAnonymous]
        [HttpGet("client-hub-test")]
        public async Task<IActionResult> SignalClientHub()
        {
            await _hubService.SignalClientHub("Hello to ClientHub!");
            return new JsonResult("ok");
        }

        [AllowAnonymous]
        [HttpGet("start-client")]
        public async Task<IActionResult> StartClient()
        {
            //await _hubClient.Start();
            await _hubService.SignalAgentHub("Hub client start");
            return new JsonResult("client start ok");
        }

        [AllowAnonymous]
        [HttpGet("stop-client")]
        public async Task<IActionResult> StopClient()
        {
            //await _hubClient.Stop();
            await _hubService.SignalAgentHub("Hub client stio");
            return new JsonResult("client stop ok");
        }

        [AllowAnonymous]
        [HttpPost("client-redirect")]
        public async Task<IActionResult> ClientRedirect([FromBody] ClientHubRedirect hubRedirect)
        {
            string url = "https://descil.ethz.ch";
            if (!string.IsNullOrEmpty(hubRedirect.DeviceId))
            {
                url = $"https://localhost:5001/start/{hubRedirect.Token}";
                await _hubService.SendDeviceRedirect(hubRedirect.DeviceId, url);
                await _hubService.SignalAgentHub($"Redirected device {hubRedirect.DeviceId}");
                return new JsonResult($"Redirected device {hubRedirect.DeviceId}");
            }

            if (!string.IsNullOrEmpty(hubRedirect.Token))
            {
                await _hubService.SendTokenRedirect(hubRedirect.Token, hubRedirect.Url);
                await _hubService.SignalAgentHub($"Redirected token {hubRedirect.Token}");
                return new JsonResult($"Redirected token to {hubRedirect.Url}");
            }

            return BadRequest();
        }

        public class ClientHubRedirect
        {
            public string Token { get; set; } = "";
            public string DeviceId { get; set; } = "";
            public string Url { get; set; } = "https://descil.ethz.ch";
        }
    }
}
