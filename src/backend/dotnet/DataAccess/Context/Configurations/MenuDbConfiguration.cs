using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Constants;
using DataAccess.Models;

namespace DataAccess.Context.Configurations;

public class MenuDbConfiguration : IEntityTypeConfiguration<MenuDb>
{
    public void Configure(EntityTypeBuilder<MenuDb> builder)
    {
        builder.ToTable("menus", t =>
        {
            t.HasCheckConstraint("CK_Menu_DescriptionLength", 
                $"char_length(description) <= {TextConstraints.MaxDescriptionLength}");
        });

        builder.HasKey(x => x.Id)
            .HasName("PK_menu");

        builder.Property(x => x.Id)
            .HasColumnName("menu_id")
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
    }
}