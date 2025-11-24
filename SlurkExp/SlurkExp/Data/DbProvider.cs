using SlurkExp.Data.Providers;

namespace SlurkExp.Data
{
    public static class DbProvider
    {
        public static IServiceCollection AddSlurkExpDbProvider(this IServiceCollection services, IConfiguration config)
        {
            var provider = config.GetValue("DbProvider", "Sqlite");

            if (string.IsNullOrEmpty(provider)) throw new ArgumentException("Provider is required");

            switch (provider)
            {
                case "Sqlite":
                    services.AddDbContext<SlurkExpDbContext, SqliteContext>();
                    break;
                case "SqlServer":
                    services.AddDbContext<SlurkExpDbContext, SqlServerContext>();
                    break;
                case "Postgres":
                    services.AddDbContext<SlurkExpDbContext, PostgresContext>();
                    break;
                default:
                    services.AddDbContext<SlurkExpDbContext, SqliteContext>();
                    break;
            }

            return services;
        }
    }
}
