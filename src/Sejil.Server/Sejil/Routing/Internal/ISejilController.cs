// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Sejil.Models.Internal;

namespace Sejil.Routing.Internal
{
    public interface ISejilController
    {
        Task GetIndexAsync();
        Task GetEventsAsync(int page, DateTime? startingTs, LogQueryFilter queryFilter);
        Task SaveQueryAsync(LogQuery logQuery);
        Task GetQueriesAsync();
        Task GetMinimumLogLevelAsync();
        void SetMinimumLogLevel(string minLogLevel);
        Task DeleteQueryAsync(string queryName);
        Task GetUserNameAsync();
        Task GetTitleAsync();
    }
}