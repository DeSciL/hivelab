using Microsoft.EntityFrameworkCore;
using SlurkExp.Models;

namespace SlurkExp.Data.Providers
{
    public class SqlServerContext : SlurkExpDbContext
    {
        public SqlServerContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
