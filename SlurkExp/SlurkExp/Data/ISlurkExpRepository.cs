using SlurkExp.Models;
using SlurkExp.Models.ViewModels;

namespace SlurkExp.Data
{
    public interface ISlurkExpRepository
    {
        IQueryable<Bot> Bots();
        IQueryable<Client> Clients();
        IQueryable<Group> Groups();
        IQueryable<LogEvent> LogEvents();
        IQueryable<Prompt> Prompts();
        IQueryable<Treatment> Treatments();

        Task<Client> GetClient(string clientToken);
        Task<Client> GetClientByTokenId(string slurkToken);
        Task SetClientStatus(string clientToken, int status);
        Task SetClientComment(string clientToken, string comment);
        Task SetClientProfile(string clientToken, ProfilePayload payload);

        Task<Client> EnsureClient(string accessToken);
        Task<Client> GetNextClient(string accessToken);
        Task<bool> EnsureGroup();

        Task AddLogEvent(string operation, string data);
        Task AddLogEvent(int clientId, string operation, string data);

        Task<int> GetClientLock();
        Task SetClientLock();
        Task<int> GetGroupLock();
        Task SetGroupLock();
        Task ReleaseLocks();
    }
}
