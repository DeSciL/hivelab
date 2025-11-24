﻿using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace SlurkExp.Data.Providers
{
    public class PostgresContext : SlurkExpDbContext
    {
        public PostgresContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Configuration.GetConnectionString("PostgresConnection"));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
