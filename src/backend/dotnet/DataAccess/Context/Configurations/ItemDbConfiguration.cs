using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccess.Models;

namespace DataAccess.Context.Configurations;

public class ItemDbConfiguration : IEntityTypeConfiguration<ItemDb>
{
    public void Configure(EntityTypeBuilder<ItemDb> builder)
    {
        builder.ToTable("items", t =>
        {
            t.HasCheckConstraint("CK_Items_Cost", "\"cost\" >= 0");
        });

        builder.HasKey(x => x.Id)
            .HasName("PK_items");

        builder.Property(x => x.Id)
            .HasColumnName("item_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(x => x.Cost)
            .HasColumnName("cost")
            .HasColumnType("numeric")
            .IsRequired();
    }
}