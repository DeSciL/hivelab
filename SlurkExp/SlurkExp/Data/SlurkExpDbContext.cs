using Microsoft.EntityFrameworkCore;

namespace SlurkExp.Data
{
    public class SlurkExpDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public SlurkExpDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public DbSet<SlurkExp.Models.Bot> Bots { get; set; }
        public DbSet<SlurkExp.Models.Client> Clients { get; set; }
        public DbSet<SlurkExp.Models.Group> Groups { get; set; }
        public DbSet<SlurkExp.Models.LogEvent> LogEvents { get; set; }
        public DbSet<SlurkExp.Models.Prompt> Prompts { get; set; }
        public DbSet<SlurkExp.Models.Treatment> Treatments { get; set; }
    }
}
