using AlertManagerWebhook.MessageBuilders;
using AlertManagerWebhook.Models;
using Microsoft.Extensions.DependencyInjection;

namespace AlertManagerWebhook.Tests.MessageBuilders;

public class MessageBuilderFactoryTests
{
    [Fact]
    public void GetMessageBuilder_Should_Return_LarkMessageBuilder_For_Lark_Receiver()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<LarkMessageBuilder>();
        services.AddTransient<DingtalkMessageBuilder>();
        var serviceProvider = services.BuildServiceProvider();

        var factory = new MessageBuilderFactory(serviceProvider);

        // Act
        var builder = factory.GetMessageBuilder<LarkMessage>(Receiver.Lark);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<LarkMessageBuilder>(builder);
    }

    [Fact]
    public void GetMessageBuilder_Should_Return_DingtalkMessageBuilder_For_Dingtalk_Receiver()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<LarkMessageBuilder>();
        services.AddTransient<DingtalkMessageBuilder>();
        var serviceProvider = services.BuildServiceProvider();

        var factory = new MessageBuilderFactory(serviceProvider);

        // Act
        var builder = factory.GetMessageBuilder<DingtalkMessage>(Receiver.Dingtalk);

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<DingtalkMessageBuilder>(builder);
    }

    [Fact]
    public void GetMessageBuilder_Should_Throw_Exception_For_Unsupported_Receiver()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<LarkMessageBuilder>();
        services.AddTransient<DingtalkMessageBuilder>();
        var serviceProvider = services.BuildServiceProvider();

        var factory = new MessageBuilderFactory(serviceProvider);

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            factory.GetMessageBuilder<LarkMessage>((Receiver)999)); // Invalid receiver
    }

    [Fact]
    public void GetMessageBuilder_Should_Throw_Exception_For_Mismatched_Message_Type()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<LarkMessageBuilder>();
        services.AddTransient<DingtalkMessageBuilder>();
        var serviceProvider = services.BuildServiceProvider();

        var factory = new MessageBuilderFactory(serviceProvider);

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            factory.GetMessageBuilder<DingtalkMessage>(Receiver.Lark)); // Mismatched type
    }
}
