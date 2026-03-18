using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StealAllTheCats.Database;
using StealAllTheCats.Services.Interfaces;
using Moq;
using StealAllTheCats.Dtos;
using StealAllTheCats.Dtos.Responses;

namespace StealAllTheCats.Tests.Integration;

public class CatCollectorFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "TestDb_" + Guid.NewGuid();
    public Mock<IApiClient> ApiClientMock { get; } = new();

    public CatCollectorFactory()
    {
        ApiClientMock.Setup(a => a.GetAsync<List<ExternalCatApiResponse>>(It.IsAny<string>()))
            .ReturnsAsync(Result<List<ExternalCatApiResponse>>.Ok(new List<ExternalCatApiResponse>
            {
                new()
                {
                    Id = "test-cat-1", Url = "https://cdn2.thecatapi.com/images/test1.jpg",
                    Width = 800, Height = 600,
                    Breeds = [new CatBreed { Temperament = "Playful, Friendly, Curious" }]
                },
                new()
                {
                    Id = "test-cat-2", Url = "https://cdn2.thecatapi.com/images/test2.jpg",
                    Width = 640, Height = 480,
                    Breeds = [new CatBreed { Temperament = "Friendly, Calm" }]
                },
                new()
                {
                    Id = "test-cat-3", Url = "https://cdn2.thecatapi.com/images/test3.jpg",
                    Width = 1024, Height = 768,
                    Breeds = [new CatBreed { Temperament = "Energetic, Playful" }]
                }
            }));

        ApiClientMock.Setup(a => a.DownloadBytesAsync(It.IsAny<string>()))
            .ReturnsAsync(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all EF/DbContext registrations to avoid provider conflicts
            var efDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains("EntityFramework") == true
                         || d.ServiceType.FullName?.Contains("DbContext") == true
                         || d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                         || (d.ServiceType.IsGenericType &&
                             d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
                         || d.ServiceType == typeof(DbContextOptions))
                .ToList();

            foreach (var d in efDescriptors)
                services.Remove(d);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Replace the real API client with our mock
            var apiClientDescriptors = services
                .Where(d => d.ServiceType == typeof(IApiClient))
                .ToList();
            foreach (var d in apiClientDescriptors)
                services.Remove(d);

            services.AddSingleton<IApiClient>(ApiClientMock.Object);

            // Create the schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Development");
    }
}
