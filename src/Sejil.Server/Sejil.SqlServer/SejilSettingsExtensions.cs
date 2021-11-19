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
        var settingsInstance = (SejilSettings)settings;

        settingsInstance.SejilRepository = new SqlServerSejilRepository(settingsInstance, connectionString);
        settingsInstance.CodeGeneratorType = typeof(SqlServerCodeGenerator);

        SqlMapper.AddTypeHandler(new GuidStringHandler());
        SqlMapper.AddTypeHandler(new StringGuidHandler());

        return settings;
    }

    private class StringGuidHandler : SqlMapper.TypeHandler<string>
    {
        public override void SetValue(IDbDataParameter parameter, string value)
            => parameter.Value = value;

        public override string Parse(object value) => value is string s
            ? s
            : value?.ToString() ?? "";
    }

    private class GuidStringHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid guid)
            => parameter.Value = guid.ToString();

        public override Guid Parse(object value)
            => new((string)value);
    }
}
