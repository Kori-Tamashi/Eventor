using DataAccess.Models;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context;

public class EventorDbContext : DbContext
{
    public DbSet<UserDb> Users { get; set; }
    public DbSet<LocationDb> Locations { get; set; }
    public DbSet<EventDb> Events { get; set; }

    public EventorDbContext(DbContextOptions<EventorDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<Gender>();
        modelBuilder.HasPostgresEnum<UserRole>();
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventorDbContext).Assembly);
    }
}