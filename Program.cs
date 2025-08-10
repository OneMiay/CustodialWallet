
using CustodialWallet.Repositories;
using CustodialWallet.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Npgsql;
using System.Data;

namespace CustodialWallet
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //builder.WebHost.UseUrls("http://0.0.0.0:8080");

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

            // DB: PostgreSQL (Dapper)
            var connectionString = builder.Configuration.GetConnectionString("Postgres")
                ?? "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=custodial";
            builder.Services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
            builder.Services.AddScoped<IDbConnection>(sp => sp.GetRequiredService<NpgsqlDataSource>().CreateConnection());

            // DI
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            var app = builder.Build();

            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            var startupLogger = loggerFactory.CreateLogger("Startup");
            startupLogger.LogInformation("Starting DemoCustodialWallet in {Environment}", app.Environment.EnvironmentName);

            // Ensure DB schema with simple retry for first-run initialization
            using (var scope = app.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
                var created = false;
                for (var attempt = 1; attempt <= 10 && !created; attempt++)
                {
                    try
                    {
                        await using var conn = await dataSource.OpenConnectionAsync();
                        await using var cmd = conn.CreateCommand();
                        cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS public.users (
                            id uuid PRIMARY KEY,
                            email text NOT NULL UNIQUE,
                            balance numeric(38,18) NOT NULL
                        );";
                        await cmd.ExecuteNonQueryAsync();
                        created = true;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "DB init attempt {Attempt} failed. Retrying...", attempt);
                        await Task.Delay(TimeSpan.FromSeconds(Math.Min(5, attempt)));
                    }
                }
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

            app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

            await app.RunAsync();
        }
    }
}
