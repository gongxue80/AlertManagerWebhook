namespace AlertManagerWebhook.Models;

using System.Text.Json.Serialization;

public record DingtalkMessage
{
    [JsonPropertyName("msgtype")]
    public string MsgType { get; set; } = "markdown";
    [JsonPropertyName("markdown")]
    public DingtalkMarkdown Markdown { get; set; } = new();
}

public record DingtalkMarkdown
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
