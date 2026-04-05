using DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Core.DatabaseIntegration;

public static class DatabaseIntegrationTestConfiguration
{
    private static DbContextOptions<EventorDbContext>? _options;
    private static readonly object _lock = new();
    private static bool _migrationApplied = false;

    public static EventorDbContext CreateDbContext()
    {
        EnsureOptions();
        var context = new EventorDbContext(_options!);
        if (!_migrationApplied)
        {
            context.Database.Migrate();
            _migrationApplied = true;
        }
        return context;
    }

    private static void EnsureOptions()
    {
        if (_options != null) return;
        lock (_lock)
        {
            if (_options != null) return;
            var connectionString = DatabaseIntegrationTestInitializer.GetConnectionString();
            var services = new ServiceCollection();
            services.AddDataAccess(connectionString);
            var serviceProvider = services.BuildServiceProvider();
            _options = serviceProvider.GetRequiredService<DbContextOptions<EventorDbContext>>();
        }
    }
}