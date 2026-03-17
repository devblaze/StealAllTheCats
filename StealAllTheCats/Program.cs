using Microsoft.EntityFrameworkCore;
using StealAllTheCats.BackgroundJobs;
using StealAllTheCats.Components;
using StealAllTheCats.Database;
using StealAllTheCats.Database.Models;
using StealAllTheCats.Database.Repositories;
using StealAllTheCats.Database.Repositories.Interfaces;
using StealAllTheCats.Services;
using StealAllTheCats.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddHttpClient<IApiClient, ApiClient>();
builder.Services.AddScoped<ICatService, CatService>();
builder.Services.AddScoped<ICatImportService, CatImportService>();

builder.Services.AddSingleton<ImportQueue>();
builder.Services.AddHostedService<ImportWorker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    for (int attempt = 1; attempt <= 3; attempt++)
    {
        try
        {
            logger.LogInformation("Migrating database (attempt {Attempt}/3)", attempt);
            dbContext.Database.Migrate();
            logger.LogInformation("Database migration successful");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Migration attempt {Attempt} failed", attempt);
            if (attempt < 3)
                Thread.Sleep(5000);
            else
                logger.LogCritical("Database migration failed after all attempts. App will continue but DB calls may fail.");
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program { }
