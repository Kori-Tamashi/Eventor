using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Constants;
using DataAccess.Models;

namespace DataAccess.Context.Configurations;

public class FeedbackDbConfiguration : IEntityTypeConfiguration<FeedbackDb>
{
    public void Configure(EntityTypeBuilder<FeedbackDb> builder)
    {
        builder.ToTable("feedbacks", t =>
        {
            t.HasCheckConstraint("CK_Feedback_Rate", "\"rate\" >= 1 AND \"rate\" <= 5");
            t.HasCheckConstraint("CK_Feedback_CommentLength", 
                $"char_length(comment) <= {TextConstraints.MaxCommentLength}");
        });

        builder.HasKey(f => f.Id)
            .HasName("PK_feedbacks");
        
        builder.Property(f => f.Id)
            .HasColumnName("feedback_id")
            .HasColumnType("uuid")
            .IsRequired();
        
        builder.Property(f => f.RegistrationId)
            .HasColumnName("registration_id")
            .HasColumnType("uuid")
            .IsRequired();
        
        builder.Property(f => f.Comment)
            .HasColumnName("comment")
            .HasColumnType("text")
            .IsRequired();
        
        builder.Property(f => f.Rate)
            .HasColumnName("rate")
            .HasColumnType("integer")
            .IsRequired();

        builder.HasOne(f => f.Registration)
            .WithMany(r => r.Feedbacks)
            .HasForeignKey(f => f.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}