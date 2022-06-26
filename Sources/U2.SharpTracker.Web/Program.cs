using MongoDB.Driver;
using Quartz.Impl;
using Quartz;
using U2.SharpTracker.Core;
using U2.SharpTracker.Core.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
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
