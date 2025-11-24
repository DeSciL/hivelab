using Microsoft.AspNetCore.SignalR;
using SlurkExp.Hubs;

namespace SlurkExp.Services.Hub
{
    public class HubService : IHubService
    {
        private readonly IHubContext<AgentHub> _panelHub;
        private readonly IHubContext<ClientHub> _clientHub;

        public HubService(
            IHubContext<AgentHub> panelHub,
            IHubContext<ClientHub> clientHub)
        {
            _panelHub = panelHub;
            _clientHub = clientHub;
        }

        #region AgentHub

        public async Task SignalAgentHub(string message)
        {
            await _panelHub.Clients.All.SendAsync("ReceiveSignal", "HubController", message);
        }

        public async Task SignalAgentHub(string source, string message)
        {
            await _panelHub.Clients.All.SendAsync("ReceiveSignal", source, message);
        }

        #endregion

        #region ClientHub

        public async Task SignalClientHub(string message)
        {
            await _clientHub.Clients.All.SendAsync("ReceiveSignal", "HubController", message);
        }

        public async Task SignalClientHub(string source, string message)
        {
            await _clientHub.Clients.All.SendAsync("ReceiveSignal", source, message);
        }

        public async Task PingClientHub(string message)
        {
            await _clientHub.Clients.All.SendAsync("ReceivePing", "HubService", "ping");
        }

        public async Task SendTokenRedirect(string token, string url)
        {
            await Task.CompletedTask;
            //var client = await _context.Clients.FirstOrDefaultAsync(x => x.Token.Equals(token));
            //if (client != null)
            //{
            //    await _clientHub.Clients.Client(client.ConnectionId).SendAsync("ReceiveRedirect", url);
            //}
        }

        public async Task SendDeviceRedirect(string deviceId, string url)
        {
            await Task.CompletedTask;
            //var client = await _context.Clients.FirstOrDefaultAsync(x => x.DeviceId.Equals(deviceId));
            //if (client != null)
            //{
            //    await _clientHub.Clients.Client(client.ConnectionId).SendAsync("ReceiveRedirect", url);
            //}
        }

        #endregion
    }
}
