using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccess.Models;

namespace DataAccess.Context.Configurations;

public class DayDbConfiguration : IEntityTypeConfiguration<DayDb>
{
    public void Configure(EntityTypeBuilder<DayDb> builder)
    {
        builder.ToTable("days", t => { t.HasCheckConstraint("CK_Days_Number_Positive", "\"number\" > 0"); });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Number)
            .IsRequired();

        builder.HasOne(x => x.Event)
            .WithMany(e => e.Days)
            .HasForeignKey(x => x.EventId);

        builder.HasOne(x => x.Menu)
            .WithOne(m => m.Day)
            .HasForeignKey<DayDb>(d => d.MenuId);
    }
}