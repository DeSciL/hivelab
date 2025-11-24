using Microsoft.AspNetCore.SignalR;
using SlurkExp.Services.Hub;

#nullable disable

namespace SlurkExp.Hubs
{
    public class ClientHub : Hub
    {
        private readonly IHubService _hubService;
        private readonly ILogger<ClientHub> _logger;

        public ClientHub(
            IHubService hubService,
            ILogger<ClientHub> logger)
        {
            _hubService = hubService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var deviceId = httpContext.Request.Query["deviceId"];
            var token = httpContext.Request.Query["token"];
            var cookie = httpContext.Request.Cookies[".Descil.Panel.Start"];

            //if (!string.IsNullOrEmpty(token))
            //{
            //    // Standard case where a client with a token arrives on the hub
            //    // var client = await _context.Clients.FirstOrDefaultAsync(x => x.Token.Equals(token) || x.Cookie.Equals(cookie));
            //    var client = await _context.Clients.FirstOrDefaultAsync(x => x.Token.Equals(token));
            //    if (client == null)
            //    {
            //        _context.Clients.Add(new Models.Client { ConnectionId = Context.ConnectionId, Token = token, Cookie = cookie, IsOnline = true });
            //    }
            //    else
            //    {
            //        client.Token = token;
            //        //client.Cookie = cookie;
            //        client.ConnectionId = Context.ConnectionId;
            //        client.IsOnline = true;
            //        client.LastUpdate = DateTime.Now;
            //    }
            //    await _context.SaveChangesAsync();
            //}
            //else if (!string.IsNullOrEmpty(deviceId))
            //{
            //    // Lab only setting where computers arrive with a device identifier
            //    // var client = await _context.Clients.FirstOrDefaultAsync(x => x.DeviceId.Equals(deviceId) || x.Cookie.Equals(cookie));
            //    var client = await _context.Clients.FirstOrDefaultAsync(x => x.DeviceId.Equals(deviceId));
            //    if (client == null)
            //    {
            //        _context.Clients.Add(new Models.Client { ConnectionId = Context.ConnectionId, DeviceId = deviceId, Cookie = cookie, IsOnline = true });
            //    }
            //    else
            //    {
            //        client.DeviceId = deviceId;
            //        //client.Cookie = cookie;
            //        client.ConnectionId = Context.ConnectionId;
            //        client.IsOnline = true;
            //        client.LastUpdate = DateTime.Now;
            //    }
            //    await _context.SaveChangesAsync();
            //}
            //else
            //{
            //    var client = await _context.Clients.FirstOrDefaultAsync(x => x.Cookie.Equals(cookie));
            //    if (client == null)
            //    {
            //        _context.Clients.Add(new Models.Client { ConnectionId = Context.ConnectionId, Cookie = cookie, IsOnline = true });
            //    }
            //    else
            //    {
            //        client.ConnectionId = Context.ConnectionId;
            //        client.IsOnline = true;
            //        client.LastUpdate = DateTime.Now;
            //    }
            //    await _context.SaveChangesAsync();
            //}

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

            //if(_settings.ClientHubSignal)
            //{
            //    await _hubService.SignalAgentHub($"Connected {token}");
            //    //await _hubService.SignalAgentHub($"Connected to ClientHub: {deviceId} : {token} : {cookie}");
            //}

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Task.CompletedTask;

            //var client = await _context.Clients.FirstOrDefaultAsync(x => x.ConnectionId.Equals(Context.ConnectionId));

            var httpContext = Context.GetHttpContext();
            var deviceId = httpContext?.Request.Query["deviceId"];
            var token = httpContext?.Request.Query["token"];
            var cookie = httpContext?.Request.Cookies[".Descil.Panel.Start"];

            //if (client != null)
            //{
            //    client.IsOnline = false;
            //    client.LastUpdate = DateTime.Now;
            //    await _context.SaveChangesAsync();
            //}

            //if (_settings.ClientHubSignal)
            //{
            //    await _hubService.SignalAgentHub($"Disconnected {token}");
            //}

            await base.OnDisconnectedAsync(exception);
        }

        // This gets called by the javascript client
        public async Task Pong(string user, string message)
        {
            await Task.CompletedTask;
            //await _repo.SetClientAlive(Context.ConnectionId);
        }
    }
}
