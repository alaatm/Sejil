using Sejil.Configuration;
using Sejil.SqlServer.Data;
using Sejil.SqlServer.Data.Query;

namespace Sejil.SqlServer.Test;

public class SejilSettingsExtensionsTests
{
    [Fact]
    public void UseSqlServer_sets_sqlServer_repository()
    {
        // Arrange
        var settings = new SejilSettings("/sejil", default);

        // Act
        settings.UseSqlServer("");

        // Assert
        Assert.IsType<SqlServerSejilRepository>(settings.SejilRepository);
    }

    [Fact]
    public void UseSqlServer_sets_sqlServerCodeGenerator_type()
    {
        // Arrange
        var settings = new SejilSettings("/sejil", default);

        // Act
        settings.UseSqlServer("");

        // Assert
        Assert.Equal(typeof(SqlServerCodeGenerator), settings.CodeGeneratorType);
    }
}
