// Copyright (C) 2021 Alaa Masoud
// See the LICENSE file in the project root for more information.

using System.Data;
using Dapper;
using Sejil.Configuration;
using Sejil.SqlServer.Data;
using Sejil.SqlServer.Data.Query;

namespace Sejil;

public static class SejilSettingsExtensions
{
    public static ISejilSettings UseSqlServer(this ISejilSettings settings, string connectionString)
    {
        _ = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        settings.CodeGeneratorType = typeof(SqlServerCodeGenerator);
        settings.SejilRepository = new SqlServerSejilRepository(settings, connectionString);

        SqlMapper.AddTypeHandler(new StringHandler());

        return settings;
    }

    private class StringHandler : SqlMapper.TypeHandler<string>
    {
        public override void SetValue(IDbDataParameter parameter, string value)
            => parameter.Value = value;

        public override string Parse(object value) => value is string s
            ? s
            : value?.ToString() ?? "";
    }
}
