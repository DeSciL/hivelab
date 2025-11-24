using Microsoft.AspNetCore.SignalR;

#nullable disable

namespace SlurkExp.Hubs
{
    public class AgentHub : Hub
    {
        private readonly ILogger<AgentHub> _logger;

        public AgentHub(
            ILogger<AgentHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            #region HubContext Data

            // TODO: This is for testing purposes only.

            //var context = Context.GetHttpContext();
            //var data = new 
            //{
            //    ConnectionId = Context.ConnectionId,
            //    UserName = Context.User.Identity.Name,
            //    UserIdentifier = Context.UserIdentifier,
            //    IpAddress = context.Connection.RemoteIpAddress.ToString(),
            //    IsWebSockets = context.WebSockets.IsWebSocketRequest
            //};

            //var dataJson = JsonConvert.SerializeObject(data);
            //await SendMessage(Context.ConnectionId, dataJson);

            #endregion

            await Task.CompletedTask;
            //if(_settings.AgentHubSignal)
            //{
            //    await SendSignal(Context.User.Identity.Name, $"{Context.User.Identity.Name} joined");
            //}

            //await _repo.SetAgentOnline(Context.User.Identity.Name, Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await Task.CompletedTask;
            //if (_settings.AgentHubSignal)
            //{
            //    await SendSignal(Context.User.Identity.Name, $"{Context.User.Identity.Name} left");
            //}

            //await _repo.SetAgentOffline(Context.ConnectionId);
        }

        // This gets called by the javascript client
        public async Task SendMessage(string user, string message)
        {
            var context = Context.GetHttpContext();
            await Clients.All.SendAsync("ReceiveMessage", Context.User.Identity.Name, message);
        }

        public async Task SendSignal(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveSignal", user, message);
        }

        // This gets called by the javascript client
        public async Task Pong(string user, string message)
        {
            await Task.CompletedTask;
            //await _repo.SetAgentAlive(Context.ConnectionId);
        }
    }
}
