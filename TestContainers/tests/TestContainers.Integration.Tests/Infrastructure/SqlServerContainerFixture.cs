using Testcontainers.MsSql;


public class SqlServerContainerFixture : IAsyncLifetime
{
    public const string FixtureName = "SqlServerContainerFixture";
    public MsSqlContainer SqlServerContainer { get; private set; }
    public string ConnectionString => SqlServerContainer.GetConnectionString();

    public SqlServerContainerFixture()
    {
        SqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Passw0rd!")
            .WithPortBinding(14330, 1433)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await SqlServerContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await SqlServerContainer.StopAsync();
    }
}


[CollectionDefinition(SqlServerContainerFixture.FixtureName)]
public class DatabaseCollection : ICollectionFixture<SqlServerContainerFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}