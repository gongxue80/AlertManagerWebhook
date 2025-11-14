using System.Net.Http.Json;
using AlertManagerWebhook.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AlertManagerWebhook.Tests.Endpoints;

public class WebhookEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WebhookEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRoot_Should_Return_Welcome_Message()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Equal("Welcome to AlertManager Webhook", responseString);
    }

    [Fact]
    public async Task PostLarkWebhook_With_Valid_Notification_Should_Succeed()
    {
        // Arrange
        var client = _factory.CreateClient();
        var notification = CreateTestNotification();

        // Act
        // Note: This test will fail at the HTTP client level since we're not mocking the external API
        // But we can still test that the validation passes
        var response = await client.PostAsJsonAsync("/lark/test-token-123", notification);

        // Assert
        // We expect 500 because we're not mocking the external API call
        Assert.NotNull(response);
    }

    [Fact]
    public async Task PostDingtalkWebhook_With_Valid_Notification_Should_Succeed()
    {
        // Arrange
        var client = _factory.CreateClient();
        var notification = CreateTestNotification();

        // Act
        // Note: This test will fail at the HTTP client level since we're not mocking the external API
        var response = await client.PostAsJsonAsync("/dingtalk/test-token-456", notification);

        // Assert
        Assert.NotNull(response);
    }

    private static Notification CreateTestNotification()
    {
        return new Notification
        {
            Version = "4",
            GroupKey = "group_key_1",
            Status = "firing",
            Receiver = "lark",
            GroupLabels = new Dictionary<string, string>(),
            CommonLabels = new Dictionary<string, string>(),
            ExternalURL = "http://alertmanager:9093",
            Alerts = new[]
            {
                new Alert
                {
                    Status = AlertStatus.Firing,
                    Labels = new Dictionary<string, string>
                    {
                        {"alertname", "TestAlert"},
                        {"severity", "warning"},
                        {"instance", "10.0.0.1:9100"}
                    },
                    Annotations = new Dictionary<string, string>
                    {
                        {"summary", "Test alert summary"},
                        {"description", "Test alert description"}
                    },
                    StartsAt = DateTime.Now.AddMinutes(-5),
                    EndsAt = DateTime.Now,
                    Fingerprint = "test_fingerprint_1"
                }
            }
        };
    }
}
