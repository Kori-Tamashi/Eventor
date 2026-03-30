using DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Core.DatabaseIntegration;

[TestClass]
public static class DatabaseIntegrationTestConfiguration
{
    private static EventorDbContext? _context;

    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        var services = new ServiceCollection();
        var connectionString = DatabaseIntegrationTestInitializer.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Test connection string not found");
        services.AddDataAccess(connectionString);
        var serviceProvider = services.BuildServiceProvider();
        _context = serviceProvider.GetRequiredService<EventorDbContext>();
        _context.Database.Migrate();
    }

    public static EventorDbContext GetDbContext()
    {
        return _context ?? throw new InvalidOperationException();
    }
}