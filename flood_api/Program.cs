using flood_api.Models;
using flood_api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ThaiWaterOptions>(builder.Configuration.GetSection("ThaiWater"));
builder.Services.Configure<PopnixOptions>(builder.Configuration.GetSection("Popnix"));
builder.Services.AddHttpClient(nameof(FloodDataService));
builder.Services.AddSingleton<FloodDataService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<FloodDataService>());

// The nationwide snapshot is a few MB of JSON; compression cuts it to a fraction.
builder.Services.AddResponseCompression();

var app = builder.Build();

app.UseResponseCompression();
app.UseDefaultFiles();
app.UseStaticFiles();

// Full snapshot for the map page: canals, roads, rivers, rain and per-source health.
app.MapGet("/api/flood", (FloodDataService data) => Results.Ok(data.Snapshot));

app.MapGet("/api/canals", (FloodDataService data) => Results.Ok(data.Snapshot.Canals));

// Roads currently under water (or all sensors with ?all=true), deepest first.
app.MapGet("/api/roads", (FloodDataService data, bool? all) => Results.Ok(
    data.Snapshot.Roads
        .Where(r => all == true || (!r.IsStale && r.Status is StationStatus.Warning or StationStatus.Critical))
        .OrderByDescending(r => r.DepthCm)));
app.MapGet("/api/rivers", (FloodDataService data) => Results.Ok(data.Snapshot.Rivers));
app.MapGet("/api/rain", (FloodDataService data) => Results.Ok(data.Snapshot.Rain));

app.MapGet("/api/status", (FloodDataService data) => Results.Ok(new
{
    data.Snapshot.RefreshedAt,
    data.Snapshot.Sources,
}));

app.Run();
//taskkill /F /IM flood_api.exe