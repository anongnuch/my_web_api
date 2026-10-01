using System.Text.Json.Nodes;
using flood_api.Models;

namespace flood_api.Services;

// Converts POPNIX Flood JSON (BMA drainage department canal gauges and road sensors) into map records.
// See https://flood.pop.in.th/api/ for the upstream format and terms (attribution required).
public class PopnixParser
{
    private readonly TimeSpan _staleAfter;

    public PopnixParser(TimeSpan staleAfter)
    {
        _staleAfter = staleAfter;
    }

    // Source: api_overview.php -> stations[]
    public List<CanalStation> ParseCanals(JsonNode? root, DateTimeOffset now)
    {
        var result = new List<CanalStation>();
        foreach (var item in Items(root?["stations"]))
        {
            var lat = ThaiWaterParser.Num(item["lat"]);
            var lng = ThaiWaterParser.Num(item["lng"]);
            if (lat is null || lng is null) continue;

            var measuredAt = ThaiWaterParser.Time(item["measured_at"]);
            var online = item["online"] is JsonValue v && v.TryGetValue<bool>(out var b) && b;

            result.Add(new CanalStation(
                Id: (int)(ThaiWaterParser.Num(item["id"]) ?? 0),
                Name: Str(item["name"]) ?? "",
                Canal: Str(item["river"]) ?? "",
                Lat: lat.Value,
                Lng: lng.Value,
                Value: ThaiWaterParser.Num(item["wl"]),
                WarningLevel: ThaiWaterParser.Num(item["warn"]),
                CriticalLevel: ThaiWaterParser.Num(item["crit"]),
                BankLevel: ThaiWaterParser.Num(item["bank"]),
                Trend: Str(item["trend"]),
                DeltaDay: ThaiWaterParser.Num(item["delta_day"]),
                Status: CanalStatus(Str(item["level"])),
                MeasuredAt: measuredAt,
                IsStale: !online || IsOld(measuredAt, now)));
        }
        return result;
    }

    // Source: api_roads.php -> roads[]
    public List<RoadSensor> ParseRoads(JsonNode? root, DateTimeOffset now)
    {
        var result = new List<RoadSensor>();
        foreach (var item in Items(root?["roads"]))
        {
            var lat = ThaiWaterParser.Num(item["lat"]);
            var lng = ThaiWaterParser.Num(item["lng"]);
            if (lat is null || lng is null) continue;

            var level = Str(item["level"]);
            var measuredAt = ThaiWaterParser.Time(item["measured_at"]);

            result.Add(new RoadSensor(
                Code: Str(item["code"]) ?? "",
                Name: Str(item["name"]) ?? "",
                District: Str(item["district"]) ?? "",
                IsTunnel: ThaiWaterParser.Num(item["kind"]) == 2,
                Lat: lat.Value,
                Lng: lng.Value,
                DepthCm: ThaiWaterParser.Num(item["depth"]),
                MaxDepthCm: ThaiWaterParser.Num(item["flood_max"]),
                FloodingSince: ThaiWaterParser.Time(item["since"]),
                Note: Str(item["msg_fail"]),
                Status: RoadStatus(level),
                MeasuredAt: measuredAt,
                IsStale: level == "off" || IsOld(measuredAt, now)));
        }
        return result;
    }

    // POPNIX level: ok / warn / crit / unk (no thresholds for the gauge).
    public static string CanalStatus(string? level) => level switch
    {
        "ok" => StationStatus.Normal,
        "warn" => StationStatus.Warning,
        "crit" => StationStatus.Critical,
        _ => StationStatus.Unknown,
    };

    // POPNIX level: dry / slight (>5 cm) / flood (>10 cm) / off (sensor offline).
    public static string RoadStatus(string? level) => level switch
    {
        "dry" => StationStatus.Normal,
        "slight" => StationStatus.Warning,
        "flood" => StationStatus.Critical,
        _ => StationStatus.Unknown,
    };

    private bool IsOld(DateTimeOffset? measuredAt, DateTimeOffset now) =>
        measuredAt is null || now - measuredAt.Value > _staleAfter;

    private static IEnumerable<JsonNode> Items(JsonNode? node) =>
        node is JsonArray array ? array.Where(n => n is not null).Cast<JsonNode>() : Enumerable.Empty<JsonNode>();

    private static string? Str(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var s) ? s : null;
}
