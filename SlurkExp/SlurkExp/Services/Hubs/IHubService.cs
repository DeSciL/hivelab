namespace SlurkExp.Services.Hub
{
    public interface IHubService
    {
        Task SignalAgentHub(string message);
        Task SignalAgentHub(string source, string message);
        Task SignalClientHub(string message);
        Task SignalClientHub(string source, string message);
        Task SendTokenRedirect(string token, string url);
        Task SendDeviceRedirect(string deviceId, string url);
    }
}
