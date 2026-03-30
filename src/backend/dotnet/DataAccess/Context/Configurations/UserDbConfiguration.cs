using DataAccess.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccess.Models;

namespace DataAccess.Context.Configurations;

public class UserDbConfiguration : IEntityTypeConfiguration<UserDb>
{
    public void Configure(EntityTypeBuilder<UserDb> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id)
            .HasName("PK_users");

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
            .IsUnique()
            .HasDatabaseName("IX_users_phone");

        builder.Property(x => x.Gender)
            .HasColumnName("gender")
            .HasColumnType("gender")
            .IsRequired();

        builder.Property(x => x.Role)
            .HasColumnName("role")
            .HasColumnType("user_role")
            .IsRequired();
        
        builder.HasIndex(x => x.Role)
            .HasDatabaseName("IX_users_role");

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasColumnType("varchar(255)")
            .IsRequired();
    }
}