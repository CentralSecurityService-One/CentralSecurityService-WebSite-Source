using CentralSecurityService.Common.Configuration;
using CentralSecurityService.Common.DataAccess.CentralSecurityService.Databases;
using CentralSecurityService.Common.DataAccess.CentralSecurityService.Repositories;
using CentralSecurityService.Configuration;
using Eadent.Common.Configuration;
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

            builder.Configuration.GetSection(CentralSecurityServiceSettings.SectionName).Get<CentralSecurityServiceSettings>();

            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddJsonFile(Path.Combine(CentralSecurityServiceSettings.Instance.Sensitive.DevelopmentFolder, "CentralSecurityService.settings.json"), optional: false, reloadOnChange: false);
            }
            else
            {
                builder.Configuration.AddJsonFile(Path.Combine(CentralSecurityServiceSettings.Instance.Sensitive.ProductionFolder, "CentralSecurityService.settings.json"), optional: false, reloadOnChange: false);
            }

            builder.Configuration.GetSection(CentralSecurityServiceCommonSettings.SectionName).Get<CentralSecurityServiceCommonSettings>();
            builder.Configuration.GetSection(CentralSecurityServiceSensitiveSettings.SectionName).Get<CentralSecurityServiceSensitiveSettings>();

            var centralSecurityServiceSensitiveSettings = CentralSecurityServiceSensitiveSettings.Instance;

            int databaseTypeValue = CentralSecurityServiceCommonSettings.Instance.DatabaseTypeValue;

            if (databaseTypeValue == DatabaseType.SqlServer)
            {
                Log.Information("Using SQL Server for Database: {DatabaseName}", CentralSecurityServiceCommonSettings.Instance.SqlServerDatabase.DatabaseName);

                var connectionString = CentralSecurityServiceCommonSettings.Instance.SqlServerDatabase.ConnectionString;

                services.AddDbContext<CentralSecurityServiceDatabase>(options => options.UseSqlServer(connectionString));
            }
            else if (databaseTypeValue == DatabaseType.PostgreSql)
            {
                Log.Information("Using PostgreSQL for Database: {DatabaseName}", CentralSecurityServiceCommonSettings.Instance.PostgreSqlDatabase.DatabaseName);

                var connectionString = CentralSecurityServiceCommonSettings.Instance.PostgreSqlDatabase.ConnectionString;

                services.AddDbContext<CentralSecurityServiceDatabase>(options => options.UseNpgsql(connectionString));
            }
            else
            {
                Log.Error($"Unsupported Database Type Value: {databaseTypeValue}");

                throw new InvalidOperationException($"Unsupported Database Type Value: {databaseTypeValue}");
            }

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
