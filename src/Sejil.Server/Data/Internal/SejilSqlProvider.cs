// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sejil.Configuration.Internal;
using Sejil.Models.Internal;

namespace Sejil.Data.Internal
{
    public class SejilSqlProvider : ISejilSqlProvider
    {
        private readonly string[] _nonPropertyColumns;

        public SejilSqlProvider(ISejilSettings settings) => _nonPropertyColumns = settings.NonPropertyColumns;

        public string GetSavedQueriesSql()
            => "SELECT * FROM log_query";

        public string InsertLogQuerySql()
            => "INSERT INTO log_query (name, query) VALUES (@name, @query)";

        public string DeleteQuerySql()
            => "DELETE FROM log_query WHERE name = @name";

        public string GetPagedLogEntriesSql(int page, int pageSize, DateTime? startingTimestamp, LogQueryFilter queryFilter)
        {
            if (page <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(page), "Argument must be greater than zero.");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Argument must be greater than zero.");
            }

            var timestampWhereClause = TimestampWhereClause();
            var queryWhereClause = QueryWhereClause();

            return
$@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    {timestampWhereClause}
    {queryWhereClause}{FiltersWhereClause()}
    ORDER BY timestamp DESC
    LIMIT {pageSize} OFFSET {(page - 1) * pageSize}
) l
LEFT JOIN log_property p ON l.id = p.logId
ORDER BY l.timestamp DESC, p.name";

            string TimestampWhereClause()
            {
                var hasStartingTimestampConstraint = startingTimestamp.HasValue;
                var hasDateFilter = queryFilter?.DateFilter != null || queryFilter?.DateRangeFilter != null;

                var sql = new StringBuilder();

                if (hasStartingTimestampConstraint || hasDateFilter)
                {
                    sql.Append("WHERE (");
                }

                if (hasStartingTimestampConstraint)
                {
                    sql.Append($@"timestamp <= '{startingTimestamp.Value.ToString("yyyy-MM-dd HH:mm:ss.fff")}'");
                }

                if (hasStartingTimestampConstraint && hasDateFilter)
                {
                    sql.Append(" AND ");
                }

                if (hasDateFilter)
                {
                    sql.Append(BuildDateFilter(queryFilter));
                }

                if (hasStartingTimestampConstraint || hasDateFilter)
                {
                    sql.Append(")");
                }

                return sql.ToString();
            }

            string QueryWhereClause() =>
                string.IsNullOrWhiteSpace(queryFilter?.QueryText)
                    ? ""
                    : timestampWhereClause.Length > 0
                        ? $"AND ({BuildPredicate(queryFilter.QueryText, _nonPropertyColumns)})"
                        : $"WHERE ({BuildPredicate(queryFilter.QueryText, _nonPropertyColumns)})";

            string FiltersWhereClause() =>
                string.IsNullOrWhiteSpace(queryFilter?.LevelFilter) && (!queryFilter?.ExceptionsOnly ?? true)
                    ? ""
                    : timestampWhereClause.Length > 0 || queryWhereClause.Length > 0
                        ? $" AND ({BuildFilterWhereClause(queryFilter.LevelFilter, queryFilter.ExceptionsOnly)})"
                        : $"WHERE ({BuildFilterWhereClause(queryFilter.LevelFilter, queryFilter.ExceptionsOnly)})";
        }

        private static string BuildFilterWhereClause(string levelFilter, bool exceptionsOnly)
        {
            var sp = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(levelFilter))
            {
                sp.AppendFormat("level = '{0}'", levelFilter);
            }

            if (exceptionsOnly && sp.Length > 0)
            {
                sp.Append(" AND ");
            }

            if (exceptionsOnly)
            {
                sp.Append("exception is not null");
            }

            return sp.ToString();
        }

        private static string BuildPredicate(string filterQuery, string[] nonPropertyColumns)
        {
            var sb = new StringBuilder();
            BuildPredicateCore(filterQuery, sb);
            return sb.ToString();

            void BuildPredicateCore(string query, StringBuilder sql)
            {
                // (...) && (...)  -or-  (...) and (...)
                // (...) || (...)  -or-  (...) or (...)
                var split = Regex.Split(query, @"(\(.+\))\s*(\|\||&&|and|or)\s*(\(.+\))").Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                if (split.Length != 3)
                {
                    // ... && (...)  -or-  ... and (...)
                    // ... || (...)  -or-  ... or (...)
                    split = Regex.Split(query, @"(.+)\s*(\|\||&&|and|or)\s*(\(.+\))").Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                    if (split.Length != 3)
                    {
                        // (...) && ...  -or-  (...) and ..
                        // (...) || ...  -or-  (...) or ..
                        split = Regex.Split(query, @"(\(.+\))\s*(\|\||&&|and|or)\s*(.+)").Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                        if (split.Length != 3)
                        {
                            // ... && ...  -or-  ... and ...
                            // ... || ...  -or-  ... or ...
                            split = Regex.Split(query, @"(.+)\s*(\|\||&&|and|or)\s*(.+)").Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                            if (split.Length != 3)
                            {
                                // name = value
                                // name != value
                                // name like 'value'
                                // name not like 'value'
                                split = Regex.Split(query, @"(\w+)\s*(=|!=|\s*like\s*|\s*not like\s*)\s*(.+)").Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                                if (split.Length == 3)
                                {
                                    if (nonPropertyColumns.Contains(split[0].ToLower()))
                                    {
                                        sql.AppendFormat("{0} {1} {2}",
                                            split[0], split[1].ToUpper().Trim(), split[2].Trim());
                                    }
                                    else
                                    {
                                        sql.AppendFormat("id {0} (SELECT logId FROM log_property WHERE name = '{1}' AND value {2} {3})",
                                            GetInclusionOperator(split[1].Trim().ToLower()), split[0], NegateIfNonInclusion(split[1].Trim().ToLower()), EnsureQuotes(split[2].Trim()));
                                    }
                                }
                                else if (split.Length == 1)
                                {
                                    // If we get here, then we received just a string. We will search the message column, exception column and all props for matches
                                    sql.AppendFormat(
                                        "(message LIKE '%{0}%' OR exception LIKE '%{0}%' OR " +
                                        "id in (SELECT logId FROM log_property WHERE value LIKE '%{0}%'))",
                                        split[0].Trim());
                                }
                            }
                            else
                            {
                                BuildPredicateCore(split[0], sql);

                                sql.Append(GetLogicalOperator(split[1].Trim().ToLower()));

                                BuildPredicateCore(split[2], sql);
                            }
                        }
                        else
                        {
                            // Remove leading and trainling parenthesis from left side and add sql parenthesis
                            sql.Append("(");
                            BuildPredicateCore(split[0].Substring(1, split[0].Length - 2), sql);
                            sql.Append(")");

                            sql.Append(GetLogicalOperator(split[1].Trim().ToLower()));

                            BuildPredicateCore(split[2], sql);
                        }
                    }
                    else
                    {
                        BuildPredicateCore(split[0], sql);

                        sql.Append(GetLogicalOperator(split[1].Trim().ToLower()));

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

                    sql.Append(GetLogicalOperator(split[1].Trim().ToLower()));

                    // Remove leading and trainling parenthesis from right side and add sql parenthesis
                    sql.Append("(");
                    BuildPredicateCore(split[2].Substring(1, split[2].Length - 2), sql);
                    sql.Append(")");
                }
            }
        }

        private static string BuildDateFilter(LogQueryFilter queryFilter)
        {
            if (queryFilter.DateFilter != null)
            {
                switch (queryFilter.DateFilter)
                {
                    case "5m":
                        return "timestamp >= datetime('now', '-5 Minute')";
                    case "15m":
                        return "timestamp >= datetime('now', '-15 Minute')";
                    case "1h":
                        return "timestamp >= datetime('now', '-1 Hour')";
                    case "6h":
                        return "timestamp >= datetime('now', '-6 Hour')";
                    case "12h":
                        return "timestamp >= datetime('now', '-12 Hour')";
                    case "24h":
                        return "timestamp >= datetime('now', '-24 Hour')";
                    case "2d":
                        return "timestamp >= datetime('now', '-2 Day')";
                    case "5d":
                        return "timestamp >= datetime('now', '-5 Day')";
                }
            }
            else if (queryFilter.DateRangeFilter != null)
            {
                return $"timestamp >= '{queryFilter.DateRangeFilter[0].ToString("yyyy-MM-dd")}' and timestamp < '{queryFilter.DateRangeFilter[1].ToString("yyyy-MM-dd")}'";
            }

            return "";
        }

        private static string GetLogicalOperator(string op)
            => op == "&&" || op == "and"
                ? " AND "
                : op == "||" || op == "or"
                    ? " OR "
                    // Below condition will never be reached
                    : throw new Exception("Invalid logical operator");

        private static string GetInclusionOperator(string op)
            => op == "=" || op == "like"
                ? "IN"
                : op == "!=" || op == "not like"
                    ? "NOT IN"
                    // Below condition will never be reached
                    : throw new Exception("Invalid logical operator");

        private static string NegateIfNonInclusion(string op)
            => op == "!="
                ? "="
                : op == "not like"
                    ? "LIKE"
                    : op.ToUpper();

        // "..."  -->  '...'
        //  ...   -->  '...'
        // '...'  -->  '...'
        private static string EnsureQuotes(string value)
        {
            if (value[0] == '"' && value[value.Length - 1] == '"')
            {
                return $"'{value.Substring(1, value.Length - 2)}'";
            }
            else if (value[0] != '\'' && value[value.Length - 1] != '\'')
            {
                return $"'{value}'";
            }
            else
            {
                return value;
            }
        }
    }
}