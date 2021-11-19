using System.Data;
using System.Data.Common;

namespace Sejil.Data;

internal static class DbCommandExtensions
{
    public static void AddParameterWithValue(this DbCommand command, string parameterName, object parameterValue)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Value = parameterValue;
        command.Parameters.Add(parameter);
    }

    public static void AddParameterWithType(this DbCommand command, string parameterName, DbType parameterType)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.DbType = parameterType;
        command.Parameters.Add(parameter);
    }
}
