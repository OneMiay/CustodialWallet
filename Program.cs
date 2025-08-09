
using CustodialWallet.Data;
using CustodialWallet.Repositories;
using CustodialWallet.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace CustodialWallet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Custodial Wallet API",
                    Version = "v1",
                    Description = "API for a Castodial wallet /API для Кастодиального кошелька"
                });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            });

            // DB
            var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:Sqlite") ?? "Data Source=custodial.db";
            builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connectionString));

            // DI
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            var app = builder.Build();

            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            var startupLogger = loggerFactory.CreateLogger("Startup");
            startupLogger.LogInformation("Starting DemoCustodialWallet in {Environment}", app.Environment.EnvironmentName);

            // Ensure DB
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
