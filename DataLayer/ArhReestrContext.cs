using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLayer;

/// <summary>
/// Контекст EF Core, описывающий таблицы и связи реестра недвижимости.
/// </summary>
public class ArhReestrContext : DbContext
{
    public ArhReestrContext(DbContextOptions<ArhReestrContext> options) : base(options)
    {
    }

    /// <summary>
    /// Районы города.
    /// </summary>
    public virtual DbSet<District> Districts => Set<District>();

    /// <summary>
    /// Взаимодействия клиентов и агентов по объектам.
    /// </summary>
    public virtual DbSet<Interaction> Interactions => Set<Interaction>();

    /// <summary>
    /// Статусы взаимодействий.
    /// </summary>
    public virtual DbSet<InteractionStatus> InteractionStatuses => Set<InteractionStatus>();

    /// <summary>
    /// Объекты недвижимости.
    /// </summary>
    public virtual DbSet<RealEstate> RealEstates => Set<RealEstate>();

    /// <summary>
    /// Фотографии объектов недвижимости.
    /// </summary>
    public virtual DbSet<RealEstatePhoto> RealEstatePhotos => Set<RealEstatePhoto>();

    /// <summary>
    /// Типы объектов недвижимости.
    /// </summary>
    public virtual DbSet<RealEstateType> RealEstateTypes => Set<RealEstateType>();

    /// <summary>
    /// Роли пользователей.
    /// </summary>
    public virtual DbSet<Role> Roles => Set<Role>();

    /// <summary>
    /// Дома, содержащие объекты недвижимости.
    /// </summary>
    public virtual DbSet<House> Houses => Set<House>();

    /// <summary>
    /// Улицы населённых пунктов.
    /// </summary>
    public virtual DbSet<Street> Streets => Set<Street>();

    /// <summary>
    /// Пользователи системы.
    /// </summary>
    public virtual DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настраиваем маппинг каждой сущности на таблицу и связи согласно базе данных MySQL.
        modelBuilder.Entity<District>(entity =>
        {
            entity.ToTable("Districts");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50);
        });

        modelBuilder.Entity<InteractionStatus>(entity =>
        {
            entity.ToTable("InteractionStatuses");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(30);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(20);
            entity.Property(e => e.DisplayName).HasColumnName("displayName").HasMaxLength(50);
        });

        modelBuilder.Entity<Street>(entity =>
        {
            entity.ToTable("Streets");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
        });

        modelBuilder.Entity<House>(entity =>
        {
            entity.ToTable("Houses");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StreetId).HasColumnName("streetId");
            entity.Property(e => e.DistrictId).HasColumnName("districtId");
            entity.Property(e => e.Number).HasColumnName("number").HasMaxLength(20);
            entity.Property(e => e.TotalFloors).HasColumnName("totalFloors");
            entity.Property(e => e.HasParking).HasColumnName("hasParking");
            entity.Property(e => e.HasElevator).HasColumnName("hasElevator");
            entity.Property(e => e.BuildingYear).HasColumnName("buildingYear");
            entity.Property(e => e.Latitude).HasColumnName("latitude").HasColumnType("decimal(10,7)");
            entity.Property(e => e.Longitude).HasColumnName("longitude").HasColumnType("decimal(10,7)");

            entity.HasOne(d => d.Street)
                .WithMany(p => p.Houses)
                .HasForeignKey(d => d.StreetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.District)
                .WithMany(p => p.Houses)
                .HasForeignKey(d => d.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LastName).HasColumnName("lastName").HasMaxLength(50);
            entity.Property(e => e.FirstName).HasColumnName("firstName").HasMaxLength(50);
            entity.Property(e => e.MiddleName).HasColumnName("middleName").HasMaxLength(50);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(15);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasColumnName("passwordHash").HasMaxLength(255);
            entity.Property(e => e.RoleId).HasColumnName("roleId");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt).HasColumnName("deletedAt");
            entity.Property(e => e.PhoneVerified).HasColumnName("phoneVerified");
            entity.Property(e => e.EmailVerified).HasColumnName("emailVerified");

            entity.HasOne(d => d.Role)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RealEstateType>(entity =>
        {
            entity.ToTable("RealEstateTypes");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(30);
        });

        modelBuilder.Entity<RealEstate>(entity =>
        {
            entity.ToTable("RealEstate");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AgentId).HasColumnName("agentId");
            entity.Property(e => e.TypeId).HasColumnName("typeId");
            entity.Property(e => e.HouseId).HasColumnName("houseId");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(12,2)");
            entity.Property(e => e.Rooms).HasColumnName("rooms");
            entity.Property(e => e.Area).HasColumnName("area").HasColumnType("decimal(8,2)");
            entity.Property(e => e.Floor).HasColumnName("floor");
            entity.Property(e => e.HasBalcony).HasColumnName("hasBalcony");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt).HasColumnName("deletedAt");

            entity.HasOne(d => d.Agent)
                .WithMany(p => p.RealEstates)
                .HasForeignKey(d => d.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Type)
                .WithMany(p => p.RealEstates)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.House)
                .WithMany(p => p.RealEstates)
                .HasForeignKey(d => d.HouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RealEstatePhoto>(entity =>
        {
            entity.ToTable("RealEstatePhotos");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RealEstateId).HasColumnName("realEstateId");
            entity.Property(e => e.FilePath).HasColumnName("filePath").HasMaxLength(200);
            entity.Property(e => e.FileName).HasColumnName("fileName").HasMaxLength(100);
            entity.Property(e => e.IsPrimary).HasColumnName("isPrimary");
            entity.Property(e => e.DeletedAt).HasColumnName("deletedAt");

            entity.HasOne(d => d.RealEstate)
                .WithMany(p => p.Photos)
                .HasForeignKey(d => d.RealEstateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Interaction>(entity =>
        {
            entity.ToTable("Interactions");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClientId).HasColumnName("clientId");
            entity.Property(e => e.AgentId).HasColumnName("agentId");
            entity.Property(e => e.RealEstateId).HasColumnName("realEstateId");
            entity.Property(e => e.StatusId).HasColumnName("statusId");
            entity.Property(e => e.ContactedAt).HasColumnName("contactedAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.DeletedAt).HasColumnName("deletedAt");

            entity.HasOne(d => d.Client)
                .WithMany(p => p.ClientInteractions)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Agent)
                .WithMany(p => p.AgentInteractions)
                .HasForeignKey(d => d.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.RealEstate)
                .WithMany(p => p.Interactions)
                .HasForeignKey(d => d.RealEstateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Status)
                .WithMany(p => p.Interactions)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}
