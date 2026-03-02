using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccess.Models;

namespace DataAccess.Context.Configurations;

public class FeedbackDbConfiguration : IEntityTypeConfiguration<FeedbackDb>
{
    public void Configure(EntityTypeBuilder<FeedbackDb> builder)
    {
        builder.ToTable("feedbacks",
            t => { t.HasCheckConstraint("CK_Feedback_Rate", "\"rate\" >= 1 AND \"rate\" <= 5"); });

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Comment)
            .IsRequired();

        builder.Property(f => f.Rate)
            .IsRequired();

        builder.HasOne(f => f.Registration)
            .WithMany(r => r.Feedbacks)
            .HasForeignKey(f => f.RegistrationId);
    }
}