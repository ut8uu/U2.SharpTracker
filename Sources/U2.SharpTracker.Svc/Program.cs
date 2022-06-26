using U2.SharpTracker.Core.Storage;
using U2.SharpTracker.Core;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

static IMongoDatabase CreateMongoDatabase(TrackerSvcSettings settings)
{
    var client = new MongoClient(settings.ConnectionString);
    return client.GetDatabase(settings.DatabaseName);
}