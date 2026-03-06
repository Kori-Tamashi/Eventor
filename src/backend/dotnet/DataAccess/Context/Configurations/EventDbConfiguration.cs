using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Constants;
using DataAccess.Models;

namespace DataAccess.Context.Configurations;

public class EventDbConfiguration : IEntityTypeConfiguration<EventDb>
{
    public void Configure(EntityTypeBuilder<EventDb> builder)
    {
        builder.ToTable("events", t =>
        {
            t.HasCheckConstraint("CK_Event_DaysCount", "\"days_count\" >= 0");
            t.HasCheckConstraint("CK_Event_Percent", "\"percent\" >= 0");
            t.HasCheckConstraint("CK_Event_DescriptionLength", 
                $"char_length(description) <= {TextConstraints.MaxDescriptionLength}");
        });

        builder.HasKey(x => x.Id)
            .HasName("PK_events");

        builder.Property(x => x.Id)
            .HasColumnName("event_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.LocationId)
            .HasColumnName("location_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.DaysCount)
            .HasColumnName("days_count")
            .HasColumnType("integer")
            .IsRequired();

        builder.Property(x => x.Percent)
            .HasColumnName("percent")
            .HasColumnType("numeric")
            .IsRequired();

        builder.HasOne(x => x.Location)
            .WithMany(l => l.Events)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}