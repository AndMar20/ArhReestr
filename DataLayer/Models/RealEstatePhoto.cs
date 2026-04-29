namespace DataLayer.Models;

/// <summary>
/// Фотография, прикреплённая к объекту недвижимости.
/// </summary>
public class RealEstatePhoto
{
    /// <summary>
    /// Идентификатор записи с фотографией.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ссылка на объект недвижимости, к которому относится файл.
    /// </summary>
    public int RealEstateId { get; set; }

    /// <summary>
    /// Путь к файлу на диске или в хранилище.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Имя файла, отображаемое в интерфейсе.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Признак основной фотографии объекта.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Время логического удаления фотографии.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Навигационное свойство к объекту недвижимости.
    /// </summary>
    public virtual RealEstate? RealEstate { get; set; }
}
