using System.Text;

using Microsoft.Azure.Cosmos;

using Newtonsoft.Json;

namespace CosmosCli;

public static class Utilities
{
    public static void VerboseWriteLine(bool isVerbose, string value)
    {
        if (isVerbose)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(value);
            Console.ForegroundColor = currentColor;
        }
    }

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

    public static StringBuilder FeedResponseOutput(int count, FeedResponse<dynamic> response)
    {
        var sb = new StringBuilder();
        sb.AppendLine("===============================================================================");
        sb.AppendLine($" Enumeration {count}");
        sb.AppendLine("===============================================================================");
        sb.AppendLine($" RequestCharge: {response.RequestCharge}");
        sb.AppendLine($" Count: {response.Count}");
        sb.AppendLine($" StatusCode: {response.StatusCode}");
        sb.AppendLine($" ActivityId: {response.ActivityId}");
        sb.AppendLine($" ContinuationToken: {response.ContinuationToken}");
        sb.AppendLine($" ETag: {response.ETag}");
        //Console.WriteLine("Headers: " + response.Headers);
        //Console.WriteLine("Diagnostics: " + response.Diagnostics);

        sb.AppendLine();
        return sb;
    }

    public static StringBuilder ItemResponseOutput(int count, ItemResponse<dynamic> response)
    {
        var sb = new StringBuilder();
        sb.AppendLine("===============================================================================");
        sb.AppendLine($" json {count}");
        sb.AppendLine("===============================================================================");
        sb.AppendLine($" RequestCharge: {response.RequestCharge}");
        sb.AppendLine($" StatusCode: {response.StatusCode}");
        sb.AppendLine($" ActivityId: {response.ActivityId}");
        sb.AppendLine($" ETag: {response.ETag}");
        //Console.WriteLine("Headers: " + response.Headers);
        //Console.WriteLine("Diagnostics: " + response.Diagnostics);

        sb.AppendLine();
        return sb;
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