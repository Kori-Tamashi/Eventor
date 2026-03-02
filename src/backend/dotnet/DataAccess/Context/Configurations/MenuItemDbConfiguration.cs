using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Context.Configurations;

public class MenuItemDbConfiguration: IEntityTypeConfiguration<MenuItemDb>
{
    public void Configure(EntityTypeBuilder<MenuItemDb> builder)
    {
        builder.ToTable("menu_items");

        builder.HasKey(x => new { x.MenuId, x.ItemId });
        
        builder.Property(x => x.MenuId)
            .HasColumnName("menu_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.ItemId)
            .HasColumnName("item_id")
            .HasColumnType("uuid")
            .IsRequired();
        
        builder.Property(x => x.Amount)
            .IsRequired();

        builder.HasOne(x => x.Menu)
            .WithMany(m => m.MenuItems)
            .HasForeignKey(x => x.MenuId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Item)
            .WithMany(i => i.MenuItems)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}