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

    public virtual DbSet<RealEstateStatus> RealEstateStatuses => Set<RealEstateStatus>();

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
    public virtual DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();
    public virtual DbSet<Notification> Notifications => Set<Notification>();
    public virtual DbSet<ViewingSlot> ViewingSlots => Set<ViewingSlot>();
    public virtual DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public virtual DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public virtual DbSet<Deal> Deals => Set<Deal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настраиваем маппинг каждой сущности на таблицу и связи согласно базе данных MySQL.
        modelBuilder.Entity<District>(entity =>
        {
            entity.ToTable("Districts");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("uk_district_name");
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
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("uk_street_name");
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
            entity.HasIndex(e => new { e.DistrictId, e.StreetId, e.Number }).IsUnique().HasDatabaseName("uk_house_address");
            entity.HasIndex(e => new { e.StreetId, e.Number }).HasDatabaseName("idx_house_street_number");

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


        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.ToTable("UserFavorites");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.RealEstateId).HasColumnName("realEstateId");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.HasIndex(e => new { e.UserId, e.RealEstateId }).IsUnique().HasDatabaseName("uk_user_favorite");
            entity.HasIndex(e => e.RealEstateId).HasDatabaseName("idx_favorite_real_estate");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(120);
            entity.Property(e => e.Message).HasColumnName("message").HasMaxLength(1000);
            entity.Property(e => e.LinkUrl).HasColumnName("linkUrl").HasMaxLength(500);
            entity.Property(e => e.IsRead).HasColumnName("isRead");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.HasIndex(e => new { e.UserId, e.CreatedAt }).HasDatabaseName("idx_notifications_user_time");
            entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt }).HasDatabaseName("idx_notifications_user_unread");
        });

        modelBuilder.Entity<ViewingSlot>(entity =>
        {
            entity.ToTable("ViewingSlots");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RealEstateId).HasColumnName("realEstateId");
            entity.Property(e => e.AgentId).HasColumnName("agentId");
            entity.Property(e => e.ClientId).HasColumnName("clientId");
            entity.Property(e => e.StartsAt).HasColumnName("startsAt");
            entity.Property(e => e.EndsAt).HasColumnName("endsAt");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(30);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.HasIndex(e => e.RealEstateId).HasDatabaseName("idx_slots_real_estate");
            entity.HasIndex(e => new { e.AgentId, e.StartsAt }).HasDatabaseName("idx_slots_agent_time");
            entity.HasIndex(e => new { e.RealEstateId, e.StartsAt }).HasDatabaseName("idx_slots_real_estate_time");
            entity.HasIndex(e => e.ClientId).HasDatabaseName("idx_slots_client");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("ChatMessages");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RealEstateId).HasColumnName("realEstateId");
            entity.Property(e => e.SenderId).HasColumnName("senderId");
            entity.Property(e => e.RecipientId).HasColumnName("recipientId");
            entity.Property(e => e.Message).HasColumnName("message").HasMaxLength(4000);
            entity.Property(e => e.SentAt).HasColumnName("sentAt");
            entity.Property(e => e.ReadAt).HasColumnName("readAt");
            entity.HasIndex(e => e.RealEstateId).HasDatabaseName("idx_chat_real_estate");
            entity.HasIndex(e => e.SenderId).HasDatabaseName("idx_chat_sender");
            entity.HasIndex(e => e.RecipientId).HasDatabaseName("idx_chat_recipient");
            entity.HasIndex(e => new { e.SenderId, e.RecipientId, e.SentAt }).HasDatabaseName("idx_chat_dialog_time");
            entity.HasIndex(e => e.SentAt).HasDatabaseName("idx_chat_time");
        });

        modelBuilder.Entity<RealEstateType>(entity =>
        {
            entity.ToTable("RealEstateTypes");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(30);
        });

        modelBuilder.Entity<RealEstateStatus>(entity =>
        {
            entity.ToTable("RealEstateStatuses");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(30);
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(30);
            entity.HasIndex(e => e.Code).IsUnique().HasDatabaseName("uk_real_estate_status_code");
        });

        modelBuilder.Entity<RealEstate>(entity =>
        {
            entity.ToTable("RealEstate");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AgentId).HasColumnName("agentId");
            entity.Property(e => e.TypeId).HasColumnName("typeId");
            entity.Property(e => e.HouseId).HasColumnName("houseId");
            entity.Property(e => e.StatusId).HasColumnName("statusId");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(12,2)");
            entity.Property(e => e.Rooms).HasColumnName("rooms");
            entity.Property(e => e.Area).HasColumnName("area").HasColumnType("decimal(8,2)");
            entity.Property(e => e.Floor).HasColumnName("floor");
            entity.Property(e => e.HasBalcony).HasColumnName("hasBalcony");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt).HasColumnName("deletedAt");
            entity.HasIndex(e => new { e.DeletedAt, e.CreatedAt }).HasDatabaseName("idx_real_estate_deleted_created");
            entity.HasIndex(e => new { e.HouseId, e.DeletedAt }).HasDatabaseName("idx_real_estate_house_deleted");
            entity.HasIndex(e => new { e.StatusId, e.DeletedAt }).HasDatabaseName("idx_real_estate_status_deleted");

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

            entity.HasOne(d => d.Status)
                .WithMany(p => p.RealEstates)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActorUserId).HasColumnName("actorUserId");
            entity.Property(e => e.EntityType).HasColumnName("entityType").HasMaxLength(50);
            entity.Property(e => e.EntityId).HasColumnName("entityId");
            entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(80);
            entity.Property(e => e.OldValue).HasColumnName("oldValue");
            entity.Property(e => e.NewValue).HasColumnName("newValue");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.CreatedAt }).HasDatabaseName("idx_audit_entity_time");
            entity.HasIndex(e => new { e.ActorUserId, e.CreatedAt }).HasDatabaseName("idx_audit_actor_time");

            entity.HasOne(d => d.ActorUser)
                .WithMany()
                .HasForeignKey(d => d.ActorUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Deal>(entity =>
        {
            entity.ToTable("Deals");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InteractionId).HasColumnName("interactionId");
            entity.Property(e => e.RealEstateId).HasColumnName("realEstateId");
            entity.Property(e => e.AgentId).HasColumnName("agentId");
            entity.Property(e => e.ClientId).HasColumnName("clientId");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)");
            entity.Property(e => e.Commission).HasColumnName("commission").HasColumnType("decimal(12,2)");
            entity.Property(e => e.ClosedAt).HasColumnName("closedAt");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.HasIndex(e => e.InteractionId).IsUnique().HasDatabaseName("uk_deal_interaction");
            entity.HasIndex(e => new { e.AgentId, e.ClosedAt }).HasDatabaseName("idx_deal_agent_closed");
            entity.HasIndex(e => e.ClosedAt).HasDatabaseName("idx_deal_closed");

            entity.HasOne(d => d.Interaction)
                .WithMany()
                .HasForeignKey(d => d.InteractionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.RealEstate)
                .WithMany()
                .HasForeignKey(d => d.RealEstateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Agent)
                .WithMany()
                .HasForeignKey(d => d.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Client)
                .WithMany()
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
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
            entity.HasIndex(e => new { e.RealEstateId, e.IsPrimary, e.DeletedAt }).HasDatabaseName("idx_real_estate_primary_not_deleted");

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
            entity.HasIndex(e => e.UpdatedAt).HasDatabaseName("idx_interactions_updated");
            entity.HasIndex(e => new { e.AgentId, e.StatusId, e.UpdatedAt }).HasDatabaseName("idx_interactions_agent_status_updated");

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
