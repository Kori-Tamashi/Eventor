using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Constants;

namespace DataAccess.Models;

/// <summary>
/// Модель таблицы локаций в базе данных
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
    [Key]
    [Column("location_id", TypeName = "uuid")]
    public Guid Id { get; set; }

    /// <summary>
    /// Название
    /// </summary>
    [Column("title", TypeName = "varchar(255)")]
    public string Title { get; set; }

    /// <summary>
    /// Описание
    /// </summary>
    [Column("description", TypeName = "text")]
    [MaxLength(TextConstraints.MaxDescriptionLength)]
    public string Description { get; set; }

    /// <summary>
    /// Цена аренды на 1 день
    /// </summary>
    [Column("cost", TypeName = "numeric")]
    public double Cost { get; set; }

    /// <summary>
    /// Вместимость
    /// </summary>
    [Column("capacity", TypeName = "int")]
    public int Capacity { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с мероприятиями
    /// </summary>
    public ICollection<EventDb>? Events { get; set; }
}