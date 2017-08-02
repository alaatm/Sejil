using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sejil.Data.Internal
{
    public class SejilSqlProvider : ISejilSqlProvider
    {
        public string GetSavedQueriesSql()
            => "SELECT * FROM log_query";

        public string InsertLogQuerySql()
            => "INSERT INTO log_query (name, query) VALUES (@name, @query);";

        public string GetPagedLogEntriesSql(int page, int pageSize, DateTime startingTimestamp, string query)
        {
            return
$@"SELECT l.*, p.* from 
(
    SELECT * FROM log
    {TimestampWhereClause()}
    {QueryWhereClause()}
    ORDER BY timestamp DESC
    LIMIT {pageSize} OFFSET {(page - 1) * pageSize}
) l
JOIN log_property p ON l.id = p.log_id
ORDER BY l.timestamp DESC, p.name";

            string TimestampWhereClause() => 
                startingTimestamp == default(DateTime)
                    ? ""
                    : $@"WHERE timestamp <= '{startingTimestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")}'";
            
            string QueryWhereClause() =>
                String.IsNullOrWhiteSpace(query)
                    ? ""
                    : startingTimestamp == default(DateTime)
                        ? $"WHERE {BuildPredicate(query)}"
                        : $"AND {BuildPredicate(query)}";
        }

        private static string BuildPredicate(string filterQuery)
        {
            var sb = new StringBuilder();
            BuildPredicateCore(filterQuery, sb);
            return sb.ToString();

            void BuildPredicateCore(string query, StringBuilder sql)
            {
                // (...) && (...)  -or-  (...) and (...)
                // (...) || (...)  -or-  (...) or (...)
                var split = Regex.Split(query, @"(\(.+\))\s*(\|\||&&|and|or)\s*(\(.+\))").Where(p => !String.IsNullOrWhiteSpace(p)).ToArray();
                if (split.Length != 3)
                {
                    // ... && (...)  -or-  ... and (...)
                    // ... || (...)  -or-  ... or (...)
                    split = Regex.Split(query, @"(.+)\s*(\|\||&&|and|or)\s*(\(.+\))").Where(p => !String.IsNullOrWhiteSpace(p)).ToArray();
                    if (split.Length != 3)
                    {
                        // (...) && ...  -or-  (...) and ..
                        // (...) || ...  -or-  (...) or ..
                        split = Regex.Split(query, @"(\(.+\))\s*(\|\||&&|and|or)\s*(.+)").Where(p => !String.IsNullOrWhiteSpace(p)).ToArray();
                        if (split.Length != 3)
                        {
                            // ... && ...  -or-  ... and ...
                            // ... || ...  -or-  ... or ...
                            split = Regex.Split(query, @"(.+)\s*(\|\||&&|and|or)\s*(.+)").Where(p => !String.IsNullOrWhiteSpace(p)).ToArray();
                            if (split.Length != 3)
                            {
                                // name = value
                                // name != value
                                // name like 'value'
                                // name not like 'value'
                                split = Regex.Split(query, @"(\w+)(=|!=|\s*like\s*|\s*not like\s*)(.+)").Where(p => !String.IsNullOrWhiteSpace(p)).ToArray();
                                if (split.Length == 3)
                                {
                                    sql.AppendFormat("id {0} (SELECT log_id FROM log_property WHERE name='{1}' AND value {2} {3})",
                                        GetInclusionOperator(split[1].Trim().ToLower()), split[0], NegateIfNonInclusion(split[1].Trim().ToLower()), EnsureQuotes(split[2]));
                                }
                                else if (split.Length == 1)
                                {
                                    // If we get here, then we received just a string. We will search the message column, exception column and all props for matches
                                    sql.AppendFormat(
                                        "(message LIKE '%{0}%' OR exception LIKE '%{0}%' OR " +
                                        "id in (SELECT log_id FROM log_property WHERE value LIKE '%{0}%'))",
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

        private static string GetLogicalOperator(string op)
            => op == "&&" || op == "and"
                ? " AND "
                : op == "||" || op == "or"
                    ? " OR "
                    : throw new Exception("Invalid logical operator");

        private static string GetInclusionOperator(string op)
            => op == "=" || op == "like"
                ? "IN"
                : op == "!=" || op == "not like"
                    ? "NOT IN"
                    : throw new Exception("Invalid logical operator");

        private static string NegateIfNonInclusion(string op)
            => op == "!="
                ? "="
                : op == "not like"
                    ? "like"
                    : op;

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