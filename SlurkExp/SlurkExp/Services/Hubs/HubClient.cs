namespace SlurkExp.Services.Hub
{
    public class HubClient : IHostedService
    {
        //private HubConnection _connection;
        //private IHubService _hubService;
        private readonly IServiceProvider _provider;
        private readonly ILogger<HubClient> _logger;

        public HubClient(
            IServiceProvider provider,
            ILogger<HubClient> logger)
        {
            _provider = provider;
            _logger = logger;

            //_connection.On<DateTime>(Strings.Events.TimeSent, ShowTime);

            //connection.Closed += async (error) =>
            //{
            //    await Task.Delay(new Random().Next(0, 5) * 1000);
            //    await connection.StartAsync();
            //};
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Loop is here to wait until the server is running
            while (true)
            {
                try
                {
                    //using (IServiceScope scope = _provider.CreateScope())
                    //{
                    //    _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();


                    //}

                    //_connection = new HubConnectionBuilder()
                    //    .WithUrl("http://noise.ethz.ch/agenthub")
                    //    .Build();

                    //_connection.On<string, string>("ReceiveMessage", async (user, message) =>
                    //{
                    //    //await _hubService.SignalAgentHub($"ReceiveMessage: {user}, {message}");
                    //    _logger.LogWarning("Received");
                    //});

                    // await _connection.StartAsync(cancellationToken);

                    break;
                }
                catch
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //await _connection.DisposeAsync();

            await Task.CompletedTask;
        }
    }
}
