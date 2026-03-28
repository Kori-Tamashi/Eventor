using Domain.Models;

namespace Tests.Core.Fixtures;

public class MenuFixture
{
    private Guid? _id;
    private string _title = "Default Menu";
    private string _description = "Default Description";

    public static MenuFixture Default() => new();

    public MenuFixture WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public MenuFixture WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public MenuFixture WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public Menu Build()
    {
        return new Menu(
            _id ?? Guid.NewGuid(),
            _title,
            _description
        );
    }
}