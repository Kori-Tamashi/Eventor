namespace Tests.Core.DatabaseIntegration;

public static class DatabaseIntegrationTestInitializer
{
    public static string GetConnectionString()
    {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var db = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "EventorTestDb";
        var user = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";

        return $"Host={host};Port={port};Database={db};Username={user};Password={password}";
    }
}