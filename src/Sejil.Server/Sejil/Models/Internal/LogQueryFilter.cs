// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

namespace Sejil.Models.Internal;

public sealed class LogQueryFilter
{
    public string? QueryText { get; set; }
    public string? DateFilter { get; set; }
    public List<DateTime>? DateRangeFilter { get; set; }
    public string? LevelFilter { get; set; }
    public bool ExceptionsOnly { get; set; }
}
