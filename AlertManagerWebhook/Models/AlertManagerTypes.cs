using System.Text.Json.Serialization;
namespace AlertManagerWebhook.Models;

public enum Receiver
{
    Lark,
    Dingtalk
}

public enum AlertStatus
{
    Firing,
    Resolved
}
// https://prometheus.io/docs/alerting/latest/configuration/#webhook_config
public record Alert
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AlertStatus Status { get; set; }
    public Dictionary<string, string> Labels { get; set; } = new();
    public Dictionary<string, string> Annotations { get; set; } = new();
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Fingerprint { get; set; } = string.Empty;
}



public record Notification
{
    public string Version { get; set; } = string.Empty;
    public string GroupKey { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Receiver { get; set; } = string.Empty;
    public Dictionary<string, string> GroupLabels { get; set; } = new();
    public Dictionary<string, string> CommonLabels { get; set; } = new();
    public string ExternalURL { get; set; } = string.Empty;
    public Alert[] Alerts { get; set; } = Array.Empty<Alert>();
}

public record AlertDetail
{
    public bool IsFiring { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
}
