using AlertManagerWebhook.MessageBuilders;
using AlertManagerWebhook.Models;

namespace AlertManagerWebhook.Tests.MessageBuilders;

public class LarkMessageBuilderTests
{
    [Fact]
    public void Build_Should_Create_Firing_Alert_Message()
    {
        // Arrange
        var alertDetail = new AlertDetail
        {
            IsFiring = true,
            Name = "HighCPUUsage",
            Severity = "critical",
            EnvName = "production",
            Project = "myapp",
            Instance = "10.0.0.1:9100",
            Host = "webserver-01",
            Description = "CPU usage has exceeded 90% for 5 minutes",
            StartsAt = DateTime.Now.AddMinutes(-10),
            EndsAt = DateTime.Now
        };

        var builder = new LarkMessageBuilder();

        // Act
        var result = builder.Build(alertDetail);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Card);
        Assert.NotNull(result.Card.Header);
        Assert.NotNull(result.Card.Elements);

        // Check header
        Assert.Contains("🚨 告警触发", result.Card.Header.Title.Content);
        Assert.Equal("red", result.Card.Header.Template);

        // Check elements
        Assert.NotEmpty(result.Card.Elements);
        // Verify critical fields are present
        Assert.Contains(result.Card.Elements, e =>
            e.Fields != null && e.Fields.Any(f =>
                f.Text.Content.Contains("HighCPUUsage")));
        Assert.Contains(result.Card.Elements, e =>
            e.Fields != null && e.Fields.Any(f =>
                f.Text.Content.Contains("critical")));
    }

    [Fact]
    public void Build_Should_Create_Resolved_Alert_Message()
    {
        // Arrange
        var alertDetail = new AlertDetail
        {
            IsFiring = false,
            Name = "HighCPUUsage",
            Severity = "critical",
            EnvName = "production",
            Project = "myapp",
            Instance = "10.0.0.1:9100",
            Host = "webserver-01",
            Description = "CPU usage has exceeded 90% for 5 minutes",
            StartsAt = DateTime.Now.AddMinutes(-10),
            EndsAt = DateTime.Now
        };

        var builder = new LarkMessageBuilder();

        // Act
        var result = builder.Build(alertDetail);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Card);
        Assert.NotNull(result.Card.Header);
        Assert.NotNull(result.Card.Elements);

        // Check header
        Assert.Contains("✅ 告警恢复", result.Card.Header.Title.Content);
        Assert.Equal("green", result.Card.Header.Template);
    }

    [Fact]
    public void Build_Should_Handle_Minimal_Alert_Data()
    {
        // Arrange
        var alertDetail = new AlertDetail
        {
            IsFiring = true,
            Name = "TestAlert",
            Severity = "info",
            EnvName = string.Empty,
            Project = string.Empty,
            Instance = "unknown",
            Host = string.Empty,
            Description = "Test alert description",
            StartsAt = DateTime.Now,
            EndsAt = DateTime.Now
        };

        var builder = new LarkMessageBuilder();

        // Act
        var result = builder.Build(alertDetail);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("blue", result.Card.Header.Template); // Info should be blue
    }
}
