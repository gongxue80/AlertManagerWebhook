namespace AlertManagerWebhook.Models;

using System.Text.Json.Serialization;

public record DingtalkMessage
{
    [JsonPropertyName("msgtype")]
    public string MsgType { get; init; } = "markdown";

    [JsonPropertyName("markdown")]
    public DingtalkMarkdown Markdown { get; init; } = new();
}

public record DingtalkMarkdown
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}
