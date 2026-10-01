using System.Text.Json.Nodes;
using flood_api.Models;
using Microsoft.Extensions.Options;

namespace flood_api.Services;

// Polls the upstream sources on a fixed interval and keeps the latest Bangkok snapshot in memory.
// - POPNIX Flood: BMA canal gauges and road flood sensors (fresh every 5-10 min)
// - ThaiWater: HII water-level stations and rain gauges, nationwide
// Each source is refreshed independently so one failing endpoint keeps its last good data.
public class FloodDataService : BackgroundService
{
    private readonly HttpClient _http;
    private readonly ThaiWaterOptions _thaiWater;
    private readonly PopnixOptions _popnix;
    private readonly ThaiWaterParser _thaiWaterParser;
    private readonly PopnixParser _popnixParser;
    private readonly ILogger<FloodDataService> _logger;
    private readonly Dictionary<string, SourceStatus> _sources = new();

    private volatile FloodSnapshot _snapshot = FloodSnapshot.Empty;

    public FloodDataService(
        IHttpClientFactory httpFactory,
        IOptions<ThaiWaterOptions> thaiWater,
        IOptions<PopnixOptions> popnix,
        ILogger<FloodDataService> logger)
    {
        _http = httpFactory.CreateClient(nameof(FloodDataService));
        _http.Timeout = TimeSpan.FromSeconds(30);
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("flood_api/1.0 (personal Bangkok flood map)");
        _thaiWater = thaiWater.Value;
        _popnix = popnix.Value;
        var staleAfter = TimeSpan.FromHours(_thaiWater.StaleAfterHours);
        _thaiWaterParser = new ThaiWaterParser(staleAfter);
        _popnixParser = new PopnixParser(staleAfter);
        _logger = logger;
    }

    public FloodSnapshot Snapshot => _snapshot;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(1, _thaiWater.RefreshMinutes)));
        do
        {
            await RefreshAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    public async Task RefreshAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var current = _snapshot;
        var thaiWater = new Uri(_thaiWater.BaseUrl);
        var popnix = new Uri(_popnix.BaseUrl);

        var canalsTask = FetchAsync("popnix:canals", new Uri(popnix, "api_overview.php"), root => _popnixParser.ParseCanals(root, now), ct);
        var roadsTask = FetchAsync("popnix:roads", new Uri(popnix, "api_roads.php"), root => _popnixParser.ParseRoads(root, now), ct);
        var riversTask = FetchAsync("thaiwater:waterlevel", new Uri(thaiWater, "waterlevel_load"), root => _thaiWaterParser.ParseRivers(root, now), ct);
        var rainTask = FetchAsync("thaiwater:rain", new Uri(thaiWater, "rain_24h"), root => _thaiWaterParser.ParseRain(root, now), ct);
        await Task.WhenAll(canalsTask, roadsTask, riversTask, rainTask);

        List<SourceStatus> sources;
        lock (_sources) sources = _sources.Values.OrderBy(s => s.Name).ToList();

        _snapshot = new FloodSnapshot(
            Canals: canalsTask.Result ?? current.Canals,
            Roads: roadsTask.Result ?? current.Roads,
            Rivers: riversTask.Result ?? current.Rivers,
            Rain: rainTask.Result ?? current.Rain,
            Sources: sources,
            RefreshedAt: now);
    }

    private async Task<List<T>?> FetchAsync<T>(string name, Uri url, Func<JsonNode?, List<T>> parse, CancellationToken ct)
    {
        try
        {
            await using var stream = await _http.GetStreamAsync(url, ct);
            var root = await JsonNode.ParseAsync(stream, cancellationToken: ct);
            var items = parse(root);
            lock (_sources) _sources[name] = new SourceStatus(name, DateTimeOffset.UtcNow, null, items.Count);
            _logger.LogInformation("Fetched {Source}: {Count} stations", name, items.Count);
            return items;
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Failed to fetch {Source}", name);
            lock (_sources)
            {
                _sources.TryGetValue(name, out var previous);
                _sources[name] = new SourceStatus(name, previous?.LastSuccess, ex.Message, previous?.Count ?? 0);
            }
            return null;
        }
    }
}
