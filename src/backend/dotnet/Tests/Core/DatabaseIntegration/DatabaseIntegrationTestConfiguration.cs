using DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Core.DatabaseIntegration;

public static class DatabaseIntegrationTestConfiguration
{
    private static EventorDbContext? _context;
    private static readonly object _lock = new();

    public static EventorDbContext GetDbContext()
    {
        if (_context != null)
            return _context;

        lock (_lock)
        {
            if (_context != null)
                return _context;

            var connectionString = DatabaseIntegrationTestInitializer.GetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Test connection string not found");

            var services = new ServiceCollection();
            services.AddDataAccess(connectionString);
            var serviceProvider = services.BuildServiceProvider();
            _context = serviceProvider.GetRequiredService<EventorDbContext>();
            _context.Database.Migrate();
        }

        return _context;
    }
}