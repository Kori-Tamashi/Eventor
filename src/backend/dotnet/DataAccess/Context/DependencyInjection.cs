using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using DataAccess.Enums;

namespace DataAccess.Context;

public static class DependencyInjection
{
    private static NpgsqlDataSource? _dataSource;
    private static readonly object _lock = new();

    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        string connectionString)
    {
        if (_dataSource == null)
        {
            lock (_lock)
            {
                if (_dataSource == null)
                {
                    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

                    dataSourceBuilder.MapEnum<GenderDb>("gender");
                    dataSourceBuilder.MapEnum<UserRoleDb>("user_role");
                    dataSourceBuilder.MapEnum<RegistrationTypeDb>("registration_type");

                    _dataSource = dataSourceBuilder.Build();
                }
            }
        }

        services.AddDbContext<EventorDbContext>(options =>
            options
                .UseLazyLoadingProxies()
                .UseNpgsql(_dataSource));

        return services;
    }
}