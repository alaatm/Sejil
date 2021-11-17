// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;

namespace Sejil.Data.Query.Internal
{
    internal static class QueryEngine
    {
        public static string Translate(string filter)
        {
            // If enclosed in quotes then treat as a search string
            if (filter[0] == '"' && filter[^1] == '"')
            {
                return string.Format("(message LIKE '%{0}%' OR exception LIKE '%{0}%' OR id in (SELECT logId FROM log_property WHERE value LIKE '%{0}%'))", filter[1..^1].Replace("'", "''"));
            }

            // If has no whitespace, "=", "!", "(", ")" then treat as a search string
            if (!Regex.IsMatch(filter, @"\s|=|!|\(|\)"))
            {
                return string.Format("(message LIKE '%{0}%' OR exception LIKE '%{0}%' OR id in (SELECT logId FROM log_property WHERE value LIKE '%{0}%'))", filter.Replace("'", "''"));
            }

            var scanner = new Scanner(filter);
            var tokens = scanner.Scan();

            return new CodeGenerator().Generate(new Parser(tokens).Parse());
        }
    }
}
