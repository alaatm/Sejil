// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Sejil.Data;
using Sejil.Logging.Sinks;
using Serilog.Events;

namespace Sejil.Test.Logging.Sinks;

public class SejilSinkTests
{
    [Fact]
    public async Task EmitBatchAsync_throws_when_null_events()
    {
        // Arrange
        var sink = new SejilSinkMock(Mock.Of<ISejilRepository>());

        // Act & assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sink.CallEmitBatchAsync(null));
    }

    [Fact]
    public async Task EmitBatchAsync_calls_repository_insert_events()
    {
        // Arrange
        var repositoryMoq = new Mock<ISejilRepository>(MockBehavior.Strict);
        repositoryMoq.Setup(p => p.InsertEventsAsync(Array.Empty<LogEvent>())).Returns(Task.CompletedTask);
        var sink = new SejilSinkMock(repositoryMoq.Object);

        // Act
        await sink.CallEmitBatchAsync(Array.Empty<LogEvent>());

        // Assert
        repositoryMoq.VerifyAll();
    }

    class SejilSinkMock : SejilSink
    {
        public SejilSinkMock(ISejilRepository repository) : base(repository) { }
        public Task CallEmitBatchAsync(IEnumerable<LogEvent> events) => EmitBatchAsync(events);
    }
}
