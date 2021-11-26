using Sejil.Background;
using Sejil.Configuration;
using Sejil.Data;
using Serilog.Events;

namespace Sejil.Test.Background;

public class EventsCleanupServiceTests
{
    [Fact]
    public async Task Service_fires_up_cleanup_task()
    {
        // Arrange
        var cleanupCalled = false;
        var repositoryMoq = new Mock<ISejilRepository>();
        repositoryMoq.Setup(p => p.CleanupAsync()).Callback(() => cleanupCalled = true);

        var settingsMoq = new Mock<ISejilSettings>(MockBehavior.Strict);
        settingsMoq.SetupGet(p => p.SejilRepository).Returns(repositoryMoq.Object);
        settingsMoq.SetupGet(p => p.RetentionPolicies).Returns(new List<RetentionPolicy>
        {
            new(TimeSpan.FromMinutes(2), new[] { LogEventLevel.Debug })
        });
        var service = new EventsCleanupService(settingsMoq.Object);

        // Act
        await service.StartAsync(default);

        // Assert
        await Task.Delay(10); // Wait a bit to trigger callback
        settingsMoq.VerifyAll();
        Assert.True(cleanupCalled);
    }
}
