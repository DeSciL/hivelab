using SlurkExp.Models.ViewModels;

namespace SlurkExp.Services.AppCache
{
    public interface IAppCache
    {
        //Task Load();
        List<BotPrompt> BotPrompts();
        Task UpdatePrompt(string id, string prompt);
    }
}
