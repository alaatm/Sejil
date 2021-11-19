// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

namespace Sejil.Data.Query;

internal sealed class QueryEngineException : Exception
{
    public QueryEngineException() { }
    public QueryEngineException(string message) : base(message) { }
    public QueryEngineException(string message, Exception innerException) : base(message, innerException) { }
}
