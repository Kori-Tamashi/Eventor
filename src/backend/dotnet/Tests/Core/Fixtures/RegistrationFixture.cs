using Domain.Enums;
using Domain.Models;

namespace Tests.Core.Fixtures;

public class RegistrationFixture
{
    private Guid? _id;
    private Guid? _eventId;
    private Guid? _userId;
    private RegistrationType _type = RegistrationType.Standard;
    private bool _payment = false;

    public static RegistrationFixture Default() => new();

    public RegistrationFixture WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public RegistrationFixture WithEventId(Guid eventId)
    {
        _eventId = eventId;
        return this;
    }

    public RegistrationFixture WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public RegistrationFixture WithType(RegistrationType type)
    {
        _type = type;
        return this;
    }

    public RegistrationFixture WithPayment(bool payment)
    {
        _payment = payment;
        return this;
    }

    public Registration Build()
    {
        return new Registration(
            _id ?? Guid.NewGuid(),
            _eventId ?? Guid.NewGuid(),
            _userId ?? Guid.NewGuid(),
            _type,
            _payment
        );
    }
}