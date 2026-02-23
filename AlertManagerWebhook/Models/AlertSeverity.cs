namespace AlertManagerWebhook.Models;

internal static class AlertSeverity
{
    public static string GetLarkTemplateColor(AlertDetail alert)
    {
        if (!alert.IsFiring)
            return "green";
        return alert.Severity.ToLower() switch
        {
            "critical" or "error" => "red",
            "warning" => "orange",
            "info" or "notice" => "blue",
            _ => "red",
        };
    }

    public static string GetDingtalkColor(AlertDetail alert)
    {
        if (!alert.IsFiring)
            return "#008000";
        return alert.Severity.ToLower() switch
        {
            "critical" or "error" => "#FF0000",
            "warning" => "#FFA500",
            "info" or "notice" => "#00BFFF",
            _ => "#FF0000",
        };
    }
}
