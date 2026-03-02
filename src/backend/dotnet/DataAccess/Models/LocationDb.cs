using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DataAccess.Models;

/// <summary>
/// Локация
/// </summary>
[Table("locations")]
public class LocationDb
{
    public LocationDb(Guid id, 
        string title, 
        string description, 
        double cost, 
        int capacity)
    {
        Id = id;
        Title = title;
        Description = description;
        Cost = cost;
        Capacity = capacity;
    }
    
    /// <summary>
    /// Идентификатор локации
    /// </summary>
    /// <example>f0fe5f0b-cfad-4caf-acaf-f6685c3a5fc6</example>
    [Key]
    [Column("location_id", TypeName = "uuid")]
    public Guid Id { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    /// <example>Коттедж</example>
    [Column("title", TypeName = "varchar(255)")]
    public string Title { get; set; }

    /// <summary>
    /// Описание
    /// </summary>
    /// <example>Домик у озера</example>
    [Column("description", TypeName = "text")]
    public string Description { get; set; }

    /// <summary>
    /// Цена аренды на 1 день
    /// </summary>
    /// <example>1000</example>
    [Column("cost", TypeName = "numeric")]
    public double Cost { get; set; }

    /// <summary>
    /// Вместимость
    /// </summary>
    /// <example>1000</example>
    [Column("capacity", TypeName = "int")]
    public int Capacity { get; set; }
    
    public ICollection<EventDb> Events { get; set; }
}
