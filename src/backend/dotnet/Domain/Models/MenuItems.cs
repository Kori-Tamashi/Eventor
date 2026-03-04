namespace Eventor.Domain.Models;

public class MenuItems
{
    public MenuItems() { }
    
    public MenuItems(Guid menuId, Guid itemId, double amount)
    {
        MenuId = menuId;
        ItemId = itemId;
        Amount = amount;
    }
    
    public Guid MenuId { get; set; }
    public Guid ItemId { get; set; }
    public double Amount { get; set; }
}