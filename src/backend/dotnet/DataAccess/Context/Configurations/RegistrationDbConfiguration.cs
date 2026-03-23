using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccess.Models;

namespace DataAccess.Context.Configurations;

public class RegistrationDbConfiguration : IEntityTypeConfiguration<RegistrationDb>
{
    public void Configure(EntityTypeBuilder<RegistrationDb> builder)
    {
        builder.ToTable("registrations");

        builder.HasKey(x => x.Id)
            .HasName("PK_registrations");
        
        builder.Property(x => x.Id)
            .HasColumnName("registration_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.EventId)
            .HasColumnName("event_id")
            .HasColumnType("uuid")
            .IsRequired();
        
        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();
        
        builder.HasIndex(x => new { x.EventId, x.UserId })
            .IsUnique()
            .HasDatabaseName("IX_registrations_event_user");

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasColumnType("registration_type")
            .IsRequired();

        builder.Property(x => x.Payment)
            .HasColumnName("payment")
            .HasColumnType("boolean")
            .IsRequired();

        builder.HasOne(x => x.Event)
            .WithMany(e => e.Registrations)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(u => u.Registrations)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}