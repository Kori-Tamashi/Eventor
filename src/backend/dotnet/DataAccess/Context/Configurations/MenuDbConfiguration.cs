using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccess.Models;

namespace DataAccess.Context.Configurations;

public class MenuDbConfiguration : IEntityTypeConfiguration<MenuDb>
{
    public void Configure(EntityTypeBuilder<MenuDb> builder)
    {
        builder.ToTable("menu");

        builder.HasKey(x => x.Id);

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