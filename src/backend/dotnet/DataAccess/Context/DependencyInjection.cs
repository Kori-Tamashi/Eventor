using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using DataAccess.Enums;

namespace DataAccess.Context;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

        dataSourceBuilder.MapEnum<GenderDb>("gender");
        dataSourceBuilder.MapEnum<UserRoleDb>("user_role");
        dataSourceBuilder.MapEnum<RegistrationTypeDb>("registration_type");

        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<EventorDbContext>(options =>
            options
                .UseLazyLoadingProxies()
                .UseNpgsql(dataSource));

        return services;
    }
}