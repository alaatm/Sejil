// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Sejil.Data.Internal;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Sejil.Logging.Sinks;

internal class SejilSink : PeriodicBatchingSink
{
    private const int DefaultBatchSizeLimit = 50;
    private static readonly TimeSpan _defaultBatchEmitPeriod = TimeSpan.FromSeconds(5);
    private readonly SejilRepository _repository;

    public SejilSink(SejilRepository repository) : base(DefaultBatchSizeLimit, _defaultBatchEmitPeriod)
        => _repository = repository;

    protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        await _repository.InsertEventsAsync(events);
    }
}
