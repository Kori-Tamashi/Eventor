using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DataAccess.Context;
using Tests.Core.DatabaseIntegration;

namespace Tests.Core;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> 
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<EventorDbContext>));
            
            if (descriptor != null)
                services.Remove(descriptor);
            var connectionString = DatabaseIntegrationTestInitializer.GetConnectionString();
            services.AddDataAccess(connectionString);
            
            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EventorDbContext>();
            context.Database.Migrate();
        });
    }
}