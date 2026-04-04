using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using DataAccess.Models;
using DataAccess.Enums;

namespace DataAccess.Context;

public class EventorDbContext : DbContext
{
    public DbSet<UserDb> Users { get; set; }
    public DbSet<LocationDb> Locations { get; set; }
    public DbSet<EventDb> Events { get; set; }
    public DbSet<ItemDb> Items { get; set; }
    public DbSet<MenuDb> Menus { get; set; }
    public DbSet<MenuItemDb> MenuItems { get; set; }
    public DbSet<RegistrationDb> Registrations { get; set; }
    public DbSet<ParticipationDb> Participations { get; set; }
    public DbSet<FeedbackDb> Feedbacks { get; set; }
    public DbSet<DayDb> Days { get; set; }


    public EventorDbContext(DbContextOptions<EventorDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<GenderDb>();
        modelBuilder.HasPostgresEnum<UserRoleDb>();
        modelBuilder.HasPostgresEnum<RegistrationTypeDb>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventorDbContext).Assembly);
    }
}