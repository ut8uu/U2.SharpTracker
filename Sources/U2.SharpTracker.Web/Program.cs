using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using U2.SharpTracker.Core.Storage;
using U2.SharpTracker.Core;
using U2.SharpTracker.Web.Data;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<U2SharpTrackerWebContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("U2SharpTrackerWebContext") ?? throw new InvalidOperationException("Connection string 'U2SharpTrackerWebContext' not found.")));

var svcSettings = new TrackerSvcSettings();
builder.Configuration.GetSection(nameof(TrackerSvcSettings)).Bind(svcSettings);

builder.Services.AddSingleton<ITrackerSvcSettings>(svcSettings);
builder.Services.AddSingleton(svcSettings);

builder.Services.AddTransient<IStorage, TrackerStorage>();

builder.Services.AddSingleton(serviceProvider =>
{
    var db = CreateMongoDatabase(svcSettings);
    var logger = serviceProvider.GetService<ILogger<SharpTrackerService>>();

    logger.LogInformation(
        $"Connected to '{svcSettings.DatabaseName}' using '{svcSettings.ConnectionString}'");
    return db;
});

builder.Services.AddScoped<ISharpTrackerService, SharpTrackerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

static IMongoDatabase CreateMongoDatabase(TrackerSvcSettings settings)
{
    var client = new MongoClient(settings.ConnectionString);
    return client.GetDatabase(settings.DatabaseName);
}
