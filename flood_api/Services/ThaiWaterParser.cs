using System.Globalization;
using System.Text.Json.Nodes;
using flood_api.Models;

namespace flood_api.Services;

// Converts raw ThaiWater JSON (nationwide) into map-ready station records.
// ThaiWater mixes numbers and numeric strings, so all reads go through tolerant helpers.
public class ThaiWaterParser
{
    private static readonly TimeSpan ThaiOffset = TimeSpan.FromHours(7);

    private readonly TimeSpan _staleAfter;

    public ThaiWaterParser(TimeSpan staleAfter)
    {
        _staleAfter = staleAfter;
    }

    public List<RiverStation> ParseRivers(JsonNode? root, DateTimeOffset now)
    {
        var result = new List<RiverStation>();
        foreach (var item in Items(root?["waterlevel_data"]?["data"]))
        {
            var station = item["station"];
            var lat = Num(station?["tele_station_lat"]);
            var lng = Num(station?["tele_station_long"]);
            if (lat is null || lng is null) continue;

            var situation = (int?)Num(item["situation_level"]);
            var percent = Num(item["storage_percent"]);
            var measuredAt = Time(item["waterlevel_datetime"]);

            result.Add(new RiverStation(
                Id: (int)(Num(station?["id"]) ?? 0),
                Name: ThaiText(station?["tele_station_name"]),
                District: ThaiText(item["geocode"]?["amphoe_name"]),
                Province: ThaiText(item["geocode"]?["province_name"]),
                Lat: lat.Value,
                Lng: lng.Value,
                WaterLevelMsl: Num(item["waterlevel_msl"]),
                BankPercent: percent,
                SituationLevel: situation,
                Status: RiverStatus(situation, percent),
                MeasuredAt: measuredAt,
                IsStale: IsStale(measuredAt, now)));
        }
        return result;
    }

    public List<RainStation> ParseRain(JsonNode? root, DateTimeOffset now)
    {
        var result = new List<RainStation>();
        foreach (var item in Items(root?["data"]))
        {
            var station = item["station"];
            var lat = Num(station?["tele_station_lat"]);
            var lng = Num(station?["tele_station_long"]);
            if (lat is null || lng is null) continue;

            var measuredAt = Time(item["rainfall_datetime"]);
            result.Add(new RainStation(
                Id: (int)(Num(station?["id"]) ?? 0),
                Name: ThaiText(station?["tele_station_name"]),
                District: ThaiText(item["geocode"]?["amphoe_name"]),
                Province: ThaiText(item["geocode"]?["province_name"]),
                Lat: lat.Value,
                Lng: lng.Value,
                Rain1h: Num(item["rain_1h"]),
                Rain24h: Num(item["rain_24h"]),
                MeasuredAt: measuredAt,
                IsStale: IsStale(measuredAt, now)));
        }
        return result;
    }

    // ThaiWater situation_level: 1-3 low/normal, 4 high (70-100% of bank), 5 overflowing bank.
    // Falls back to the bank-fill percentage when the level is missing.
    public static string RiverStatus(int? situationLevel, double? bankPercent)
    {
        if (situationLevel is >= 1 and <= 5)
        {
            return situationLevel switch
            {
                5 => StationStatus.Critical,
                4 => StationStatus.Warning,
                _ => StationStatus.Normal,
            };
        }
        if (bankPercent is null) return StationStatus.Unknown;
        if (bankPercent >= 100) return StationStatus.Critical;
        if (bankPercent >= 70) return StationStatus.Warning;
        return StationStatus.Normal;
    }

    private bool IsStale(DateTimeOffset? measuredAt, DateTimeOffset now) =>
        measuredAt is null || now - measuredAt.Value > _staleAfter;

    private static IEnumerable<JsonNode> Items(JsonNode? node) =>
        node is JsonArray array ? array.Where(n => n is not null).Cast<JsonNode>() : Enumerable.Empty<JsonNode>();

    private static string ThaiText(JsonNode? node) =>
        Str(node?["th"]) ?? Str(node?["en"]) ?? "";

    private static string? Str(JsonNode? node)
    {
        if (node is not JsonValue value) return null;
        return value.TryGetValue<string>(out var s) ? s : value.ToJsonString();
    }

    public static double? Num(JsonNode? node)
    {
        if (node is not JsonValue value) return null;
        if (value.TryGetValue<double>(out var d)) return d;
        if (value.TryGetValue<string>(out var s) &&
            double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            return parsed;
        return null;
    }

    // ThaiWater timestamps are local Thai time without an offset, e.g. "2026-10-01 15:40".
    public static DateTimeOffset? Time(JsonNode? node)
    {
        var s = Str(node);
        if (s is null) return null;
        string[] formats = { "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss" };
        return DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var local)
            ? new DateTimeOffset(local, ThaiOffset)
            : null;
    }
}
