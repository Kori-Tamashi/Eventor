using Domain.Models;

namespace Tests.Core.Fixtures;

public class LocationFixture
{
    private Guid? _id;
    private string _title = "Default Location";
    private string _description = "Default Description";
    private decimal _cost = 100m;
    private int _capacity = 50;

    public static LocationFixture Default() => new();

    public LocationFixture WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public LocationFixture WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public LocationFixture WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public LocationFixture WithCost(decimal cost)
    {
        _cost = cost;
        return this;
    }

    public LocationFixture WithCapacity(int capacity)
    {
        _capacity = capacity;
        return this;
    }

    public Location Build()
    {
        return new Location(
            _id ?? Guid.NewGuid(),
            _title,
            _description,
            _cost,
            _capacity
        );
    }
}