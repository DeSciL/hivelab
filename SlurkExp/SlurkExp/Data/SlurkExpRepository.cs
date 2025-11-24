using Microsoft.EntityFrameworkCore;
using SlurkExp.Models;
using SlurkExp.Models.ViewModels;
using SlurkExp.Services.Settings;
using SlurkExp.Services.SlurkSetup;
using System.Text.Json;

namespace SlurkExp.Data
{
    public class SlurkExpRepository : ISlurkExpRepository
    {
        private readonly SlurkExpDbContext _context;
        private readonly ISettingsService _settingsService;
        private readonly ISlurkSetup _slurkSetup;
        private readonly ILogger<SlurkExpRepository> _logger;

        private static SemaphoreSlim clientSemaphoreSlim = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim groupSemaphoreSlim = new SemaphoreSlim(1, 1);

        public SlurkExpRepository(
            SlurkExpDbContext context,
            ISlurkSetup slurkSetup,
            ISettingsService settingsService,
            ILogger<SlurkExpRepository> logger)
        {
            _context = context;
            _slurkSetup = slurkSetup;
            _settingsService = settingsService;
            _logger = logger;
        }

        #region IQueryables

        public IQueryable<Bot> Bots()
        {
            return _context.Bots;
        }

        public IQueryable<Client> Clients()
        {
            return _context.Clients;
        }

        public IQueryable<Group> Groups()
        {
            return _context.Groups;
        }

        public IQueryable<LogEvent> LogEvents()
        {
            return _context.LogEvents;
        }

        public IQueryable<Prompt> Prompts()
        {
            return _context.Prompts;
        }

        public IQueryable<Treatment> Treatments()
        {
            return _context.Treatments;
        }

        #endregion

        public async Task<Client> GetClient(string clientToken)
        {
            return await _context.Clients.Include(g => g.Group).Include(t => t.Group.Treatment).FirstOrDefaultAsync(c => c.ClientToken == clientToken);
        }

        public async Task<Client> GetClientByTokenId(string chatToken)
        {
            return await _context.Clients.FirstOrDefaultAsync(c => c.ChatToken == chatToken);
        }

        public async Task SetClientStatus(string clientToken, int status)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientToken == clientToken);
            if (client != null)
            {
                client.Status = status;
                client.Updated = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetClientComment(string clientToken, string comment)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientToken == clientToken);
            if (client != null)
            {
                client.Comment = comment;
                client.Updated = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetClientProfile(string clientToken, ProfilePayload payload)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientToken == clientToken);
            if (client != null)
            {
                client.ClientJson = JsonSerializer.Serialize(payload);
                client.ChatName = payload.Name;
                client.Updated = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Client> EnsureClient(string accessCode)
        {
            Client client = await _context.Clients
                .Include(g => g.Group)
                .Include(t => t.Group.Treatment)
                .Where(c => c.AccessCode == accessCode)
                .FirstOrDefaultAsync();

            if (client != null) return client;

            client = await GetNextClient(accessCode);

            return client;
        }

        public async Task<Client> GetNextClient(string accessToken)
        {
            await clientSemaphoreSlim.WaitAsync();
            Client client = null;

            try
            {
                client = await _context.Clients
                .Include(i => i.Group)
                .Include(i => i.Group.Treatment)
                .Where(c => c.AccessCode == "")
                .FirstOrDefaultAsync();

                if (client != null)
                {
                    // Make reservation
                    client.AccessCode = accessToken;
                    client.Status = 1;
                    client.Group.Checkin++;
                    await _context.SaveChangesAsync();
                }
            }
            finally
            {
                clientSemaphoreSlim.Release();
            }

            if (client is not null) return client;

            // Add a new group
            var result = await EnsureGroup();
            return await GetNextClient(accessToken);
        }

        public async Task<bool> EnsureGroup()
        {
            Group group = new Group();
            await groupSemaphoreSlim.WaitAsync();

            try
            {
                if (_settingsService.IsRandomDispatch)
                {
                    group = await _context.Groups
                        .OrderBy(g => g.SortGroup)
                        .ThenBy(g => g.SortOrder)
                        .Where(g => g.Status.Equals(0))
                        .FirstOrDefaultAsync();
                }
                else
                {
                    group = await _context.Groups
                        .OrderBy(g => g.GroupId)
                        .Where(g => g.Status.Equals(0))
                        .FirstOrDefaultAsync();
                }

                if (group != null)
                {
                    SlurkSetupResponse slurkTokens;

                    if (group.Bots > 0)
                    {
                        // TODO: Does not handle the case where botcount > 1
                        slurkTokens = await _slurkSetup.RoomSetup(group.Seats, group.Seats - 2, new List<int> { 1 }, new List<string> { "Ash" });
                    }
                    else
                    {
                        slurkTokens = await _slurkSetup.RoomSetup(group.Seats, group.Seats - 2, new List<int>(), new List<string>());
                    }

                    foreach (var token in slurkTokens.UserTokens)
                    {
                        var client = new Client
                        {
                            Status = 0,
                            ChatToken = token,
                            GroupId = group.GroupId
                        };

                        _context.Clients.Add(client);
                    }

                    group.Status = 1;
                    group.WaitingRoomId = slurkTokens.WaitingRoomId;
                    group.ChatRoomId = slurkTokens.ChatRoomId;
                    await _context.SaveChangesAsync();
                }
            }
            finally
            {
                groupSemaphoreSlim.Release();
            }

            if (group is null) return false;
            return true;
        }

        public async Task AddLogEvent(string operation, string data)
        {
            _context.LogEvents.Add(new LogEvent
            {
                Operation = operation,
                Data = data
            });
            await _context.SaveChangesAsync();
        }

        public async Task AddLogEvent(int clientId, string operation, string data)
        {
            _context.LogEvents.Add(new LogEvent
            {
                ClientId = clientId,
                Operation = operation,
                Data = data
            });
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetClientLock()
        {
            await Task.CompletedTask;
            return clientSemaphoreSlim.CurrentCount;
        }

        public async Task SetClientLock()
        {
            await clientSemaphoreSlim.WaitAsync();
        }

        public async Task<int> GetGroupLock()
        {
            await Task.CompletedTask;
            return groupSemaphoreSlim.CurrentCount;
        }

        public async Task SetGroupLock()
        {
            await groupSemaphoreSlim.WaitAsync();
        }

        public async Task ReleaseLocks()
        {
            await Task.CompletedTask;

            if (clientSemaphoreSlim.CurrentCount == 0)
            {
                clientSemaphoreSlim.Release();
            }
            if (groupSemaphoreSlim.CurrentCount == 0)
            {
                groupSemaphoreSlim.Release();
            }
        }

    }
}
