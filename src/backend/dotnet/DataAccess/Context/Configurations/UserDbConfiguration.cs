using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Context.Configurations;

public class UserDbConfiguration : IEntityTypeConfiguration<UserDb> 
{
    public void Configure(EntityTypeBuilder<UserDb> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.HasIndex(x => x.Phone)
            .IsUnique();

        builder.Property(x => x.Gender)
            .HasColumnName("gender")
            .HasColumnType("gender")
            .IsRequired();

        builder.Property(x => x.Role)
            .HasColumnName("role")
            .HasColumnType("user_role")
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasColumnType("varchar(255)")
            .IsRequired();
    }
}