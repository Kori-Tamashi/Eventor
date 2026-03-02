using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Context.Configurations;

public class LocationDbConfiguration : IEntityTypeConfiguration<LocationDb>
{
    public void Configure(EntityTypeBuilder<LocationDb> builder)
    {
        builder.ToTable("locations", t => 
        {
            t.HasCheckConstraint("CK_Location_Cost", "\"cost\" >= 0");
            t.HasCheckConstraint("CK_Location_Capacity", "\"capacity\" >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("location_id")
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

        builder.Property(x => x.Cost)
            .HasColumnName("cost")
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.Capacity)
            .HasColumnName("capacity")
            .HasColumnType("int")
            .IsRequired();
        
        builder.HasMany(x => x.Events)
            .WithOne(e=> e.Location)
            .HasForeignKey(x => x.LocationId);
    }
}