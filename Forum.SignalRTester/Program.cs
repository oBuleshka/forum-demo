using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var settings = builder.Configuration.GetSection("SignalRTester").Get<SignalRTesterOptions>() ?? new SignalRTesterOptions();
var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    WriteIndented = true
};

Console.WriteLine("Forum SignalR Tester");
Console.WriteLine("--------------------");

var hubUrl = Prompt("Hub URL", settings.HubUrl);
var userIdInput = Prompt("UserId (display only)", string.Empty);
var postIdInput = Prompt("PostId", string.Empty);
var tokenInput = Prompt("JWT token (optional, required if hub is authorized)", settings.AccessToken ?? string.Empty, secret: true);

if (!Guid.TryParse(postIdInput, out var postId))
{
    Console.WriteLine("Invalid PostId. Please restart and enter a valid GUID.");
    return;
}

var displayUserId = string.IsNullOrWhiteSpace(userIdInput) ? "UnknownUser" : userIdInput.Trim();
var accessToken = string.IsNullOrWhiteSpace(tokenInput) ? null : tokenInput.Trim();

var connection = new HubConnectionBuilder()
    .WithUrl(hubUrl, options =>
    {
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
        }
    })
    .WithAutomaticReconnect()
    .Build();

connection.On<JsonElement>("PostUpdated", payload =>
{
    var message = payload.Deserialize<PostUpdatedMessage>(jsonOptions);
    var eventType = message?.Type ?? "(unknown)";
    var serializedPayload = JsonSerializer.Serialize(payload, jsonOptions);

    Console.WriteLine();
    Console.WriteLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] [{displayUserId}] RECEIVED EVENT");
    Console.WriteLine($"Type: {eventType}");
    Console.WriteLine("Payload:");
    Console.WriteLine(serializedPayload);
    Console.WriteLine();
});

connection.Reconnecting += error =>
{
    Console.WriteLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] Reconnecting: {error?.Message ?? "connection interrupted"}");
    return Task.CompletedTask;
};

connection.Reconnected += connectionId =>
{
    Console.WriteLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] Reconnected. ConnectionId: {connectionId}");
    return connection.InvokeAsync("JoinPostGroup", postId);
};

connection.Closed += async error =>
{
    Console.WriteLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] Connection closed: {error?.Message ?? "closed"}");
    Console.WriteLine("Attempting to reconnect in 5 seconds...");
    await Task.Delay(TimeSpan.FromSeconds(5));

    try
    {
        await connection.StartAsync();
        await connection.InvokeAsync("JoinPostGroup", postId);
        Console.WriteLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] Reconnected and rejoined post group.");
    }
    catch (Exception reconnectException)
    {
        Console.WriteLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] Reconnect failed: {reconnectException.Message}");
    }
};

try
{
    await connection.StartAsync();
    Console.WriteLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] Connected to {hubUrl}");

    await connection.InvokeAsync("JoinPostGroup", postId);
    Console.WriteLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] Joined post group: {postId}");
    Console.WriteLine("Listening for 'PostUpdated' events. Press Q to quit.");

    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Q)
        {
            break;
        }
    }
}
catch (Exception exception)
{
    Console.WriteLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] Connection error: {exception.Message}");
}
finally
{
    await connection.DisposeAsync();
}

static string Prompt(string label, string defaultValue, bool secret = false)
{
    var suffix = string.IsNullOrWhiteSpace(defaultValue) ? string.Empty : $" [{defaultValue}]";
    Console.Write($"{label}{suffix}: ");

    if (!secret)
    {
        var enteredValue = Console.ReadLine();
        return string.IsNullOrWhiteSpace(enteredValue) ? defaultValue : enteredValue.Trim();
    }

    var buffer = new List<char>();
    while (true)
    {
        var keyInfo = Console.ReadKey(intercept: true);
        if (keyInfo.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (keyInfo.Key == ConsoleKey.Backspace)
        {
            if (buffer.Count > 0)
            {
                buffer.RemoveAt(buffer.Count - 1);
            }

            continue;
        }

        if (!char.IsControl(keyInfo.KeyChar))
        {
            buffer.Add(keyInfo.KeyChar);
        }
    }

    var secretValue = new string(buffer.ToArray());
    return string.IsNullOrWhiteSpace(secretValue) ? defaultValue : secretValue.Trim();
}

internal sealed class SignalRTesterOptions
{
    public string HubUrl { get; set; } = "https://localhost:7108/hubs/posts";
    public string? AccessToken { get; set; }
}

internal sealed class PostUpdatedMessage
{
    public string? Type { get; set; }
}
