// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

namespace Sejil.Models.Internal;

public sealed class LogEntryProperty
{
    public int Id { get; set; }
    public string LogId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Value { get; set; }
}
