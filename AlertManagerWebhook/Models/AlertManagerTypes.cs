namespace AlertManagerWebhook.Models;

public enum Receiver
{
    Lark,
    Dingtalk
}

public record Alert
{
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, string> Labels { get; set; } = new();
    public Dictionary<string, string> Annotations { get; set; } = new();
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Fingerprint { get; set; } = string.Empty;
    public int Count { get; set; }
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