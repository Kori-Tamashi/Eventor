using Domain.Models;

namespace Tests.Core.Fixtures;

public class FeedbackFixture
{
    private Guid? _id;
    private Guid? _registrationId;
    private string _comment = "Default comment";
    private int _rate = 5;

    public static FeedbackFixture Default() => new();

    public FeedbackFixture WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public FeedbackFixture WithRegistrationId(Guid registrationId)
    {
        _registrationId = registrationId;
        return this;
    }

    public FeedbackFixture WithComment(string comment)
    {
        _comment = comment;
        return this;
    }

    public FeedbackFixture WithRate(int rate)
    {
        _rate = rate;
        return this;
    }

    public Feedback Build()
    {
        return new Feedback(
            _id ?? Guid.NewGuid(),
            _registrationId ?? Guid.NewGuid(),
            _comment,
            _rate
        );
    }
}