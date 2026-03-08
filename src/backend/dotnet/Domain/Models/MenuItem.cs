namespace Eventor.Domain.Models;

public class MenuItem
{
    public MenuItem() { }
    
    public MenuItem(Guid itemId, int amount)
    {
        ItemId = itemId;
        Amount = amount;
    }
    public Guid ItemId { get; set; }
    public int Amount { get; set; }
}