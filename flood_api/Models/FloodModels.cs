namespace flood_api.Models;

public class ThaiWaterOptions
{
    public string BaseUrl { get; set; } = "https://api-v3.thaiwater.net/api/v1/thaiwater30/public/";
    public int RefreshMinutes { get; set; } = 5;
    public int StaleAfterHours { get; set; } = 3;
}

public class PopnixOptions
{
    public string BaseUrl { get; set; } = "https://flood.pop.in.th/";
}

// Status values shared by the API and the map legend.
public static class StationStatus
{
    public const string Normal = "normal";
    public const string Warning = "warning";
    public const string Critical = "critical";
    public const string Unknown = "unknown";
}

// BMA drainage canal gauge. Levels are metres above mean sea level, not water depth.
public record CanalStation(
    int Id,
    string Name,
    string Canal,
    double Lat,
    double Lng,
    double? Value,
    double? WarningLevel,
    double? CriticalLevel,
    double? BankLevel,
    string? Trend,
    double? DeltaDay,
    string Status,
    DateTimeOffset? MeasuredAt,
    bool IsStale);

// BMA road / tunnel flood sensor. Depth is centimetres of water on the road surface.
public record RoadSensor(
    string Code,
    string Name,
    string District,
    bool IsTunnel,
    double Lat,
    double Lng,
    double? DepthCm,
    double? MaxDepthCm,
    DateTimeOffset? FloodingSince,
    string? Note,
    string Status,
    DateTimeOffset? MeasuredAt,
    bool IsStale);

public record RiverStation(
    int Id,
    string Name,
    string District,
    string Province,
    double Lat,
    double Lng,
    double? WaterLevelMsl,
    double? BankPercent,
    int? SituationLevel,
    string Status,
    DateTimeOffset? MeasuredAt,
    bool IsStale);

public record RainStation(
    int Id,
    string Name,
    string District,
    string Province,
    double Lat,
    double Lng,
    double? Rain1h,
    double? Rain24h,
    DateTimeOffset? MeasuredAt,
    bool IsStale);

public record SourceStatus(string Name, DateTimeOffset? LastSuccess, string? LastError, int Count);

public record FloodSnapshot(
    IReadOnlyList<CanalStation> Canals,
    IReadOnlyList<RoadSensor> Roads,
    IReadOnlyList<RiverStation> Rivers,
    IReadOnlyList<RainStation> Rain,
    IReadOnlyList<SourceStatus> Sources,
    DateTimeOffset? RefreshedAt)
{
    public static readonly FloodSnapshot Empty = new(
        Array.Empty<CanalStation>(), Array.Empty<RoadSensor>(), Array.Empty<RiverStation>(),
        Array.Empty<RainStation>(), Array.Empty<SourceStatus>(), null);
}
