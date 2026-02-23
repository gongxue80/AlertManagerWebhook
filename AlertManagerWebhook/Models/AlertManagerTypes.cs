using System.Text.Json.Serialization;

namespace AlertManagerWebhook.Models;

public enum Receiver
{
    Lark,
    Dingtalk,
}

public enum AlertStatus
{
    Firing,
    Resolved,
}

// https://prometheus.io/docs/alerting/latest/configuration/#webhook_config
public record Alert
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AlertStatus Status { get; init; }
    public Dictionary<string, string> Labels { get; init; } = new();
    public Dictionary<string, string> Annotations { get; init; } = new();
    public DateTime StartsAt { get; init; }
    public DateTime EndsAt { get; init; }
    public string Fingerprint { get; init; } = string.Empty;
}

public record Notification
{
    public string Version { get; init; } = string.Empty;
    public string GroupKey { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Receiver { get; init; } = string.Empty;
    public Dictionary<string, string> GroupLabels { get; init; } = new();
    public Dictionary<string, string> CommonLabels { get; init; } = new();
    public string ExternalURL { get; init; } = string.Empty;
    public Alert[] Alerts { get; init; } = Array.Empty<Alert>();
}

public record AlertDetail
{
    public bool IsFiring { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string EnvName { get; init; } = string.Empty;
    public string Project { get; init; } = string.Empty;
    public string Instance { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime StartsAt { get; init; }
    public DateTime EndsAt { get; init; }
}
