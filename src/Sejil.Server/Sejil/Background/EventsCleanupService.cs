using Microsoft.Extensions.Hosting;
using Sejil.Configuration;
using Sejil.Data;

namespace Sejil.Background;

internal sealed class EventsCleanupService : IHostedService, IDisposable
{
    private readonly SejilRepository _repository;
    private readonly int _invokeDuration;
    private Timer _timer = null!;

    public EventsCleanupService(ISejilSettings settings)
    {
        _repository = settings.SejilRepository;
        _invokeDuration = settings.MinimumSchedulerTimerInMinutes;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(_invokeDuration));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void DoWork(object? _) => _ = _repository.CleanupAsync();

    public void Dispose() => _timer?.Dispose();
}
