﻿using Microsoft.EntityFrameworkCore;
using SlurkExp.Models;

namespace SlurkExp.Data.Providers
{
    public class SqliteContext : SlurkExpDbContext
    {
        public SqliteContext(IConfiguration configuration) : base(configuration)
        {
            this.Database.Migrate();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(Configuration.GetConnectionString("SqliteConnection"));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
