using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SlurkExp.Data;
using SlurkExp.Data.SlurkDb;
using SlurkExp.Hubs;
using SlurkExp.Services;
using SlurkExp.Services.ApiKey;
using SlurkExp.Services.AppCache;
using SlurkExp.Services.Hub;
using SlurkExp.Services.Settings;
using SlurkExp.Services.SlurkSetup;

namespace SlurkExp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Logging
            builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services));

            var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(defaultConnection));
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddSlurkExpDbProvider(builder.Configuration);
            builder.Services.AddDbContext<SlurkDbContext>();

            builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection("ApiKeyOptions"));
            builder.Services.Configure<SlurkSetupOptions>(builder.Configuration.GetSection("SlurkSetupOptions"));
            builder.Services.Configure<SurveyOptions>(builder.Configuration.GetSection("SurveyOptions"));
            builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            builder.Services.AddRazorPages();
            builder.Services.AddSignalR();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IAppCache, AppCache>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddTransient<IHubService, HubService>();

            builder.Services.AddTransient<ISlurkSetup,SlurkSetup>();
            builder.Services.AddTransient<ISlurkExpRepository, SlurkExpRepository>();
            builder.Services.AddCustomDataProtection();
            builder.Services.AddCloudscribePagination();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // app.UseHsts();
            }

            app.UseSerilogRequestLogging();

            //app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "text/plain"
            });

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();
            app.MapDefaultControllerRoute();
            app.MapHub<AgentHub>("/agentHub");
            app.MapHub<ClientHub>("/clientHub");

            app.Run();
        }
    }
}
