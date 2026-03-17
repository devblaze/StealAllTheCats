using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Data;
using StealAllTheCats.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFastEndpoints()
    .SwaggerDocument();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<ICatService, CatService>(client =>
{
    client.BaseAddress = new Uri("https://api.thecatapi.com/v1/");
    client.DefaultRequestHeaders.Add("x-api-key", "YOUR_CAT_API_KEY");
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

// app.UseHttpsRedirection();
// app.UseAuthorization(); 
app.UseFastEndpoints();

app.Run();