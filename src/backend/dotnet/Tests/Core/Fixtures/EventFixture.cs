using Domain.Models;

namespace Tests.Core.Fixtures;

public class EventFixture
{
    private Guid? _id;
    private Guid? _locationId;
    private string _title = "Default Event";
    private string _description = "Default Description";
    private DateOnly _startDate = DateOnly.FromDateTime(DateTime.UtcNow);
    private int _daysCount = 1;
    private double _percent = 0;

    public static EventFixture Default() => new();

    public EventFixture WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public EventFixture WithLocationId(Guid locationId)
    {
        _locationId = locationId;
        return this;
    }

    public EventFixture WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public EventFixture WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public EventFixture WithStartDate(DateOnly date)
    {
        _startDate = date;
        return this;
    }

    public EventFixture WithDaysCount(int days)
    {
        _daysCount = days;
        return this;
    }

    public EventFixture WithPercent(double percent)
    {
        _percent = percent;
        return this;
    }

    public Event Build()
    {
        return new Event(
            _id ?? Guid.NewGuid(),
            _locationId ?? Guid.NewGuid(),
            _title,
            _description,
            _startDate,
            _daysCount,
            _percent
        );
    }
}