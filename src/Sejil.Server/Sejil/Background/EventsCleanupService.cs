using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Sejil.Configuration;
using Sejil.Data;

namespace Sejil.Background;

internal sealed class EventsCleanupService : IHostedService, IDisposable
{
    private readonly ISejilRepository _repository;
    private readonly int _invokeDuration;
    private Timer _timer = null!;

    public EventsCleanupService(ISejilSettings settings)
    {
        _repository = settings.SejilRepository;
        _invokeDuration = SchedulingHelper.GetTimerDuration(settings.RetentionPolicies);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Debug.Assert(_invokeDuration >= 1);
        _timer = new Timer(Callback, null, TimeSpan.Zero, TimeSpan.FromMinutes(_invokeDuration));
        return Task.CompletedTask;

        void Callback(object? _) => _ = _repository.CleanupAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}
