namespace Eventor.Domain.Models;

public class MenuItems
{
    public MenuItems() { }
    
    public MenuItems(Guid menuId, 
        Guid itemId, 
        int amount)
    {
        MenuId = menuId;
        ItemId = itemId;
        Amount = amount;
    }
    
    public Guid MenuId { get; set; }
    public Guid ItemId { get; set; }
    public int Amount { get; set; }
}