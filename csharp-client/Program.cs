using System.Net.Http.Headers;
using System.Text.Json;

if (args.Length == 0 && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("API_KEY"))) {
    Console.WriteLine("Usage: dotnet run -- <API_KEY>\nOr set API_KEY environment variable.");
    return;
}

var apiKey = args.Length > 0 ? args[0] : Environment.GetEnvironmentVariable("API_KEY");
var baseUrl = args.Length > 1 ? args[1] : "http://localhost:3000";

using var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

try
{
    var resp = await client.PostAsync(baseUrl + "/api/validate-key", null);
    var txt = await resp.Content.ReadAsStringAsync();
    if (!resp.IsSuccessStatusCode)
    {
        Console.WriteLine($"Validation failed: {resp.StatusCode} - {txt}");
        return;
    }

    using var doc = JsonDocument.Parse(txt);
    var root = doc.RootElement;
    Console.WriteLine("Key valid. Response:");
    Console.WriteLine(JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true }));
}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex.Message);
}
