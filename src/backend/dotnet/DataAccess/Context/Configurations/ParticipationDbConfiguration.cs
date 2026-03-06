using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccess.Models;

namespace DataAccess.Context.Configurations;

public class ParticipationDbConfiguration : IEntityTypeConfiguration<ParticipationDb>
{
    public void Configure(EntityTypeBuilder<ParticipationDb> builder)
    {
        builder.ToTable("participation");

        builder.HasKey(x => new { x.DayId, x.RegistrationId })
            .HasName("PK_participation");
        
        builder.Property(x => x.DayId)
            .HasColumnName("day_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.RegistrationId)
            .HasColumnName("registration_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasOne(x => x.Day)
            .WithMany(d => d.Participations)
            .HasForeignKey(x => x.DayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Registration)
            .WithMany(r => r.Participations)
            .HasForeignKey(x => x.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}