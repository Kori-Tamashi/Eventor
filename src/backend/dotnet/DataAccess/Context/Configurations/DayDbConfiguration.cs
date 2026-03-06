using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Constants;
using DataAccess.Models;

namespace DataAccess.Context.Configurations;

public class DayDbConfiguration : IEntityTypeConfiguration<DayDb>
{
    public void Configure(EntityTypeBuilder<DayDb> builder)
    {
        builder.ToTable("days", t =>
        {
            t.HasCheckConstraint("CK_Days_SequenceNumber_Positive", "\"sequence_number\" > 0");
            t.HasCheckConstraint("CK_Days_DescriptionLength", 
                $"char_length(description) <= {TextConstraints.MaxDescriptionLength}");
        });

        builder.HasKey(x => x.Id)
            .HasName("PK_days");
        
        builder.Property(x => x.Id)
            .HasColumnName("day_id")
            .HasColumnType("uuid")
            .IsRequired();
        
        builder.Property(x => x.EventId)
            .HasColumnName("event_id")
            .HasColumnType("uuid")
            .IsRequired();
        
        builder.Property(x => x.MenuId)
            .HasColumnName("menu_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(x => x.SequenceNumber)
            .HasColumnName("sequence_number")
            .HasColumnType("integer")
            .IsRequired();
        
        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .IsRequired();

        builder.HasOne(x => x.Event)
            .WithMany(e => e.Days)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Menu)
            .WithOne(m => m.Day)
            .HasForeignKey<DayDb>(d => d.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}