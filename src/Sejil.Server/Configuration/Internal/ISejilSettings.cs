// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using Serilog.Events;
using Serilog.Core;
using System.IO;

namespace Sejil.Configuration.Internal
{
    public interface ISejilSettings
    {
        string SejilAppHtml { get; }
        string Url { get; }
        LoggingLevelSwitch LoggingLevelSwitch { get; }
        string SqliteDbPath { get; }
        string[] NonPropertyColumns { get; }
        int PageSize { get; }
        bool TrySetMinimumLogLevel(string minLogLevel);

#if NETSTANDARD2_0
        /// <summary>
        /// Gets or sets the authentication scheme, used for the index page. Leave empty for no authentication.
        /// </summary>
        string AuthenticationScheme { get; set; }
#endif
    }
}