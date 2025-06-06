using CentralSecurityService.Configuration;
using CentralSecurityService.DataAccess.CentralSecurityService.Databases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
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

            builder.Configuration.GetSection(CentralSecurityServiceSettings.SectionName).Get<CentralSecurityServiceSettings>();

            services.AddDbContext<CentralSecurityServiceDatabase>(options => options.UseSqlServer(CentralSecurityServiceSettings.Instance.Database.ConnectionString));

            services.AddScoped<ICentralSecurityServiceDatabase, CentralSecurityServiceDatabase>();

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
