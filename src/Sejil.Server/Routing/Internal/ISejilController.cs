// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sejil.Models.Internal;

namespace Sejil.Routing.Internal
{
    public interface ISejilController
    {
        Task GetIndexAsync(HttpContext context);
        Task GetEventsAsync(HttpContext context, int page, DateTime? startingTs, LogQueryFilter queryFilter);
        Task SaveQueryAsync(HttpContext context, LogQuery logQuery);
        Task GetQueriesAsync(HttpContext context);
        void SetMinimumLogLevel(HttpContext context, string minLogLevel);
        Task DeleteQueryAsync(HttpContext context, string queryName);
    }
}