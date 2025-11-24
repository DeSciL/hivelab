using SlurkExp.Models.ViewModels;

namespace SlurkExp.Services.AppCache
{
    public class AppCache : IAppCache
    {
        private readonly IWebHostEnvironment _env;
        private readonly IServiceProvider _provider;
        private readonly ILogger<AppCache> _logger;
        private List<BotPrompt> _botPrompts { get; set; } = new List<BotPrompt>();

        public AppCache(
            IWebHostEnvironment env,
            IServiceProvider provider,
            ILogger<AppCache> logger)
        {
            _env = env;
            _provider = provider;
            _logger = logger;

            Load().Wait();
        }

        public async Task Load()
        {
            await Task.CompletedTask;

            _logger.LogInformation("Loading bot prompts");

            _botPrompts.Clear();
            _botPrompts.Add(new BotPrompt { Id = (_botPrompts.Count).ToString(), Prompt = $"Sound like a priate!" });

            string results = "";
            string file = "/prompts/";
            string dirPath = System.IO.Path.Join(_env.WebRootPath, file);

            var files = ListFilesInDirectory(dirPath);
            foreach (var f in files)
            {
                var fileName = Path.GetFileName(f).Replace("bot_", "").Replace(".txt", "");
                var prompt = await System.IO.File.ReadAllTextAsync(f);
                _botPrompts.Add(new BotPrompt { Id = fileName, Prompt = prompt });
                results += $"{fileName};";
            }

            _botPrompts = _botPrompts.OrderBy(x => x.Id).ToList();
        }

        public List<BotPrompt> BotPrompts()
        {
            return _botPrompts;
        }

        public Task UpdatePrompt(string id, string prompt)
        {
            var botPrompt = _botPrompts.FirstOrDefault(x => x.Id == id);
            if (botPrompt != null)
            {
                botPrompt.Prompt = prompt;
            }
            return Task.CompletedTask;
        }

        static string[] ListFilesInDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path);
            }
            else
            {
                throw new DirectoryNotFoundException($"The directory {path} does not exist.");
            }
        }
    }
}
