namespace DataLayer.Models;

/// <summary>
/// Избранные объекты пользователя, синхронизируемые между устройствами.
/// </summary>
public class UserFavorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RealEstateId { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
    public virtual RealEstate? RealEstate { get; set; }
}
