using DataAccess.Models;
using Domain.Models;

namespace DataAccess.Converters;

public static class FeedbackConverter
{
    public static Feedback? ToDomain(FeedbackDb? db)
    {
        if (db == null) return null;

        return new Feedback(
            db.Id,
            db.RegistrationId,
            db.Comment,
            db.Rate
        );
    }

    public static FeedbackDb? ToDb(Feedback? feedback)
    {
        if (feedback == null) return null;

        return new FeedbackDb(
            feedback.Id,
            feedback.RegistrationId,
            feedback.Comment,
            feedback.Rate
        );
    }
}