using Domain.Models;

namespace Tests.Core.Fixtures;

public class ItemFixture
{
    private Guid? _id;
    private string _title = "Default Item";
    private decimal _cost = 100m;

    public static ItemFixture Default() => new();

    public ItemFixture WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ItemFixture WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ItemFixture WithCost(decimal cost)
    {
        _cost = cost;
        return this;
    }

    public Item Build()
    {
        return new Item
        {
            Id = _id ?? Guid.NewGuid(),
            Title = _title,
            Cost = _cost
        };
    }
}