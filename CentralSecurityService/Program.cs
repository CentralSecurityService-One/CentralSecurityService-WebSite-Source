using CentralSecurityService.Common.Configuration;
using CentralSecurityService.Common.DataAccess.CentralSecurityService.Databases;
using CentralSecurityService.Common.DataAccess.CentralSecurityService.Repositories;
using CentralSecurityService.Configuration;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CentralSecurityService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog using appsettings.json
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog();            // Add services to the container.

            var services = builder.Services;

            services.AddRazorPages();

            builder.Configuration.AddJsonFile("SensitiveSettings/CentralSecurityService.settings.json", optional: false, reloadOnChange: false);

            builder.Configuration.GetSection(CentralSecurityServiceCommonSettings.SectionName).Get<CentralSecurityServiceCommonSettings>();

            var connectionString = CentralSecurityServiceCommonSettings.Instance.Database.ConnectionString;

            services.AddDbContext<CentralSecurityServiceDatabase>(options => options.UseSqlServer(connectionString));

            services.AddScoped<ICentralSecurityServiceDatabase, CentralSecurityServiceDatabase>();

            services.AddTransient<IReferencesRepository, ReferencesRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
