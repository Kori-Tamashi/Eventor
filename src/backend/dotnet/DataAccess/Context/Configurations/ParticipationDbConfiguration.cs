using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccess.Models;

namespace DataAccess.Context.Configurations;

public class ParticipationDbConfiguration : IEntityTypeConfiguration<ParticipationDb>
{
    public void Configure(EntityTypeBuilder<ParticipationDb> builder)
    {
        builder.ToTable("participation");

        builder.HasKey(x => new { x.DayId, x.RegistrationId });

        builder.HasIndex(x => new { x.DayId, x.RegistrationId })
            .IsUnique();

        builder.HasOne(x => x.Day)
            .WithMany(d => d.Participations)
            .HasForeignKey(x => x.DayId);

        builder.HasOne(x => x.Registration)
            .WithMany(r => r.Participations)
            .HasForeignKey(x => x.RegistrationId);
    }
}