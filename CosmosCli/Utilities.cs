using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosCli;

public static class Utilities
{
    public static void ErrorWriteLine(string value)
    {
        var currentColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(value);
        Console.ForegroundColor = currentColor;
    }

    public static void WarnWriteLine(string value)
    {
        var currentColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(value);
        Console.ForegroundColor = currentColor;
    }

    public static void WriteLine(string value)
    {
        Console.WriteLine(value);
    }

    public static string? ReadFromPipeIfNull(string? param)
    {
        if (param is null && Console.IsInputRedirected)
        {
            using StreamReader reader = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding);
            param = reader.ReadToEnd();
        }
        return param;
    }

    public static JObject FeedResponseJson(int count, FeedResponse<dynamic> response)
    {
        var jsonObject = new JObject();
        jsonObject["Enumeration"] = count;
        if (!string.IsNullOrWhiteSpace(response.ActivityId))
            jsonObject["ActivityId"] = response.ActivityId;
        jsonObject["Count"] = response.Count;
        //jsonObject["Diagnostics"] = JsonConvert.SerializeObject(response.Diagnostics);
        //jsonObject["Headers"] = JsonConvert.SerializeObject(response.Headers);
        jsonObject["RequestCharge"] = response.RequestCharge;
        jsonObject["StatusCode"] = response.StatusCode.ToString();
        if (!string.IsNullOrWhiteSpace(response.ETag))
            jsonObject["ETag"] = response.ETag;
        if (!string.IsNullOrWhiteSpace(response.ContinuationToken))
            jsonObject["ContinuationToken"] = response.ContinuationToken;
        return jsonObject;
    }

    public static JObject ItemResponseJson(int count, ItemResponse<dynamic> response)
    {
        var jsonObject = new JObject();
        jsonObject["Item"] = count;
        if (!string.IsNullOrWhiteSpace(response.ActivityId))
            jsonObject["ActivityId"] = response.ActivityId;
        //jsonObject["Diagnostics"] = JsonConvert.SerializeObject(response.Diagnostics);
        //jsonObject["Headers"] = JsonConvert.SerializeObject(response.Headers);
        jsonObject["RequestCharge"] = response.RequestCharge;
        jsonObject["StatusCode"] = response.StatusCode.ToString();
        if (!string.IsNullOrWhiteSpace(response.ETag))
            jsonObject["ETag"] = response.ETag;
        return jsonObject;
    }

    public static ConsistencyLevel? ConvertToConsistencyLevel(string? consistencyLevel)
    {
        if (consistencyLevel == null)
            return null;
        if (consistencyLevel.ToLower() == "eventual")
            return ConsistencyLevel.Eventual;
        if (consistencyLevel.ToLower() == "consistentprefix")
            return ConsistencyLevel.ConsistentPrefix;
        if (consistencyLevel.ToLower() == "session")
            return ConsistencyLevel.Session;
        if (consistencyLevel.ToLower() == "boundedstaleness")
            return ConsistencyLevel.BoundedStaleness;
        if (consistencyLevel.ToLower() == "strong")
            return ConsistencyLevel.Strong;
        return null;
    }

    public static string SerializeObject(object obj)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        return JsonConvert.SerializeObject(
            obj,
            Formatting.Indented,
            settings);
    }
}