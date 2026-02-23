using System.Reflection;
using AlertManagerWebhook;
using AlertManagerWebhook.MessageBuilders;
using AlertManagerWebhook.Models;

// Check for version argument
if (args is ["--version" or "-v"])
{
    var version = Assembly.GetExecutingAssembly().GetName().Version;
    var versionString = version is not null
        ? $"{version.Major}.{version.Minor}.{version.Build}"
        : "1.0.1";
    Console.WriteLine($"AlertManagerWebhook Version {versionString}");
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.Configure<WebhookConfig>(builder.Configuration.GetSection("WebhookConfig"));
builder.Services.AddTransient<LarkMessageBuilder>();
builder.Services.AddTransient<DingtalkMessageBuilder>();
builder.Services.AddHttpClient();

var app = builder.Build();

app.MapGet("/", () => "Welcome to AlertManager Webhook");
app.MapPost("/{receiver}/{token}", WebhookHandlers.HandleWebhookAsync);

app.Run();
