﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace LogsExplorer.Server
{
    public static class ApplicationBuilderExtensions
    {
        private const int _pageSize = 100;
        private static JsonSerializerSettings camelCaseSerializerSetting = new JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() };
        private static string logsHtml = GetEmbeddedResource("LogsExplorer.Server.index.html");

        private static ILogsExplorerOptions _options;

        public static IApplicationBuilder UseLogsExplorer(this IApplicationBuilder app, string url)
        {
            _options = app.ApplicationServices.GetService(typeof(ILogsExplorerOptions)) as ILogsExplorerOptions;

            app.UseRouter(routes =>
            {
                routes.MapGet(url, async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(logsHtml);
                });

                routes.MapPost($"{url}/events", async context =>
                {
                    var filter = await GetRequestBodyAsync(context.Request);
                    Int32.TryParse(context.Request.Query["page"].FirstOrDefault(), out var page);
                    DateTime.TryParse(context.Request.Query["startingTs"].FirstOrDefault(), out var startingTs);
                    var sql = GetSql(_options.LogsTableName, _options.LogsPropertiesTableName, page == 0 ? 1 : page, startingTs, filter);

                    using (var conn = new SqlConnection(_options.ConnectionString))
                    {
                        var lookup = new Dictionary<Guid, LogEntry>();
                        var data = conn.Query<LogEntry, LogEntryProperty, LogEntry>(sql, (l, p) =>
                            {
                                LogEntry logEntry;
                                if (!lookup.TryGetValue(l.Id, out logEntry))
                                {
                                    lookup.Add(l.Id, logEntry = l);
                                }

                                if (logEntry.Properties == null)
                                {
                                    logEntry.Properties = new List<LogEntryProperty>();
                                }
                                logEntry.Properties.Add(p);
                                return logEntry;

                            }).ToList();
                        var resultList = lookup.Values;

                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(resultList, camelCaseSerializerSetting));
                    }
                });

                routes.MapPost($"{url}/filter", async context =>
                {
                    var filter = await GetRequestBodyAsync(context.Request);
                    var filterName = context.Request.Query["name"].FirstOrDefault();
                });

                routes.MapGet($"{url}/filters", async context =>
                {
                    await context.Response.WriteAsync("");
                });
            });

            return app;
        }

        private static async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            if (request.ContentLength > 0)
            {
                var buffer = new byte[(int)request.ContentLength];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                return Encoding.Default.GetString(buffer);
            }

            return null;
        }

        private static string GetSql(string tableName, string propertiesTableName, int page, DateTime startingTs, string filter)
        {
            var sql = $@"select l.*, p.* from 
                        (
                            select * from {tableName} 
                            #TIMESTAMP_WHERE_CLAUSE#
                            #FILTER_WHERE_CLAUSE#
                            order by timestamp desc
                            offset {(page - 1) * _pageSize} rows
                            fetch next {_pageSize} rows only
                        ) l
                        join {propertiesTableName} p on l.id = p.logid
                        order by l.timestamp desc, p.[key]";

            sql = sql.Replace("#TIMESTAMP_WHERE_CLAUSE#", startingTs == default(DateTime)
                ? ""
                : $@"where timestamp <= '{startingTs.ToString("yyyy-MM-dd HH:mm:ss.fff")}'");

            if (String.IsNullOrWhiteSpace(filter))
            {
                return sql.Replace("#FILTER_WHERE_CLAUSE#", "");
            }
            else
            {
                if (startingTs == default(DateTime))
                {
                    return sql.Replace("#FILTER_WHERE_CLAUSE#", $"where {BuildPredicate(propertiesTableName, filter)}");
                }
                else
                {
                    return sql.Replace("#FILTER_WHERE_CLAUSE#", $"and {BuildPredicate(propertiesTableName, filter)}");
                }
            }
        }
        private static string BuildPredicate(string propertiesTableName, string filterQuery)
        {
            var sb = new StringBuilder();
            BuildPredicateCore(filterQuery, sb);
            return sb.ToString();

            void BuildPredicateCore(string query, StringBuilder sql)
            {
                // (...) && (...)
                // (...) || (...)
                var split = Regex.Split(query, @"(\(.+\))\s*(\|\||&&)\s*(\(.+\))").Where(p => !String.IsNullOrWhiteSpace(p)).ToArray();
                if (split.Length != 3)
                {
                    // ... && (...)
                    // ... || (...)
                    split = Regex.Split(query, @"(.+)\s*(\|\||&&)\s*(\(.+\))").Where(p => !String.IsNullOrWhiteSpace(p)).ToArray();
                    if (split.Length != 3)
                    {
                        // (...) && ...
                        // (...) || ...
                        split = Regex.Split(query, @"(\(.+\))\s*(\|\||&&)\s*(.+)").Where(p => !String.IsNullOrWhiteSpace(p)).ToArray();
                        if (split.Length != 3)
                        {
                            // ... && ...
                            // ... || ...
                            split = Regex.Split(query, @"(.+)\s*(&&|\|\|)\s*(.+)").Where(p => !String.IsNullOrWhiteSpace(p)).ToArray();
                            if (split.Length != 3)
                            {
                                // key = value
                                // key != value
                                // key like 'value'
                                // key not like 'value'
                                split = Regex.Split(query, @"(\w+)(=|!=|\s*like\s*|\s*not like\s*)(.+)").Where(p => !String.IsNullOrWhiteSpace(p)).ToArray();
                                if (split.Length == 3)
                                {
                                    sql.AppendFormat("id {0} (select logid from " + propertiesTableName + " where [key]='{1}' and [value] {2} {3})",
                                        GetInclusionOperator(split[1].Trim()), split[0], NegateIfNonInclusion(split[1].Trim()), EnsureQuotes(split[2]));
                                }
                                else if (split.Length == 1)
                                {
                                    // If we get here, then we received just a string. We will search the message column, exception column and all props for matches
                                    sql.AppendFormat(
                                        "(message like '%{0}%' or exception like '%{0}%' or " +
                                        "id in (select logid from " + propertiesTableName + " where [value] like '%{0}%'))",
                                        split[0].Trim());
                                }
                                else
                                {
                                    throw new Exception("Error parsing filter query.");
                                }
                            }
                            else
                            {
                                BuildPredicateCore(split[0], sql);

                                sql.Append(GetLogicalOperator(split[1].Trim()));

                                BuildPredicateCore(split[2], sql);
                            }
                        }
                        else
                        {
                            // Remove leading and trainling parenthesis from left side and add sql parenthesis
                            sql.Append("(");
                            BuildPredicateCore(split[0].Substring(1, split[0].Length - 2), sql);
                            sql.Append(")");

                            sql.Append(GetLogicalOperator(split[1].Trim()));

                            BuildPredicateCore(split[2], sql);
                        }
                    }
                    else
                    {
                        BuildPredicateCore(split[0], sql);

                        sql.Append(GetLogicalOperator(split[1].Trim()));

                        // Remove leading and trainling parenthesis from right side and add sql parenthesis
                        sql.Append("(");
                        BuildPredicateCore(split[2].Substring(1, split[2].Length - 2), sql);
                        sql.Append(")");
                    }
                }
                else
                {
                    // Remove leading and trainling parenthesis from left side and add sql parenthesis
                    sql.Append("(");
                    BuildPredicateCore(split[0].Substring(1, split[0].Length - 2), sql);
                    sql.Append(")");

                    sql.Append(GetLogicalOperator(split[1].Trim()));

                    // Remove leading and trainling parenthesis from right side and add sql parenthesis
                    sql.Append("(");
                    BuildPredicateCore(split[2].Substring(1, split[2].Length - 2), sql);
                    sql.Append(")");
                }
            }
        }

        private static string GetLogicalOperator(string op)
            => op == "&&"
                ? " and "
                : op == "||"
                    ? " or "
                    : throw new Exception("Invalid logical operator");

        private static string GetInclusionOperator(string op)
            => op == "=" || op == "like"
                ? "in"
                : op == "!=" || op == "not like"
                    ? "not in"
                    : throw new Exception("Invalid logical operator");

        private static string NegateIfNonInclusion(string op)
            => op == "!="
                ? "="
                : op == "not like"
                    ? "like"
                    : op;

        private static string EnsureQuotes(string value)
            => value[0] != '\'' && value[value.Length - 1] != '\''
                ? $"'{value}'"
                : value;

        private static string GetEmbeddedResource(string name)
        {
            using (var stream = typeof(ApplicationBuilderExtensions).Assembly.GetManifestResourceStream(name))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}