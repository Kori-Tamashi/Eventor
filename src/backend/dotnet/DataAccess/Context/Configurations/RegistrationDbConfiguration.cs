using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccess.Models;

namespace DataAccess.Context.Configurations;

public class RegistrationDbConfiguration : IEntityTypeConfiguration<RegistrationDb>
{
    public void Configure(EntityTypeBuilder<RegistrationDb> builder)
    {
        builder.ToTable("registrations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Payment)
            .IsRequired();

        builder.HasIndex(x => new { x.EventId, x.UserId })
            .IsUnique();

        builder.HasOne(x => x.Event)
            .WithMany(e => e.Registrations)
            .HasForeignKey(x => x.EventId);

        builder.HasOne(x => x.User)
            .WithMany(u => u.Registrations)
            .HasForeignKey(x => x.UserId);
    }
}