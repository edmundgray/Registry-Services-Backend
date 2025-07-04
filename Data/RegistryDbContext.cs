using Microsoft.EntityFrameworkCore;
using RegistryApi.Models;

namespace RegistryApi.Data;

// Using primary constructor for DbContext
public class RegistryDbContext(DbContextOptions<RegistryDbContext> options) : DbContext(options)
{
    // DbSets for Specification tables
    public DbSet<SpecificationIdentifyingInformation> SpecificationIdentifyingInformations => Set<SpecificationIdentifyingInformation>();
    public DbSet<SpecificationCore> SpecificationCores => Set<SpecificationCore>();
    public DbSet<SpecificationExtensionComponent> SpecificationExtensionComponents => Set<SpecificationExtensionComponent>();

    // DbSets for Model tables
    public DbSet<CoreInvoiceModel> CoreInvoiceModels => Set<CoreInvoiceModel>();
    public DbSet<ExtensionComponentsModelHeader> ExtensionComponentsModelHeaders => Set<ExtensionComponentsModelHeader>();
    public DbSet<ExtensionComponentModelElement> ExtensionComponentModelElements => Set<ExtensionComponentModelElement>();
    public DbSet<AdditionalRequirement> AdditionalRequirements => Set<AdditionalRequirement>();
    // New DbSets for User and UserGroup
    public DbSet<User> Users => Set<User>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Configure Composite Keys and Relationships for existing entities ---

        modelBuilder.Entity<ExtensionComponentModelElement>()
            .HasIndex(e => new { e.ExtensionComponentID, e.BusinessTermID })
            .IsUnique()
            .HasDatabaseName("UQ_ExtensionComponentModelElements_Component_BusinessTerm");

        modelBuilder.Entity<SpecificationExtensionComponent>()
            .HasOne(specExt => specExt.ExtensionComponentModelElement)
            .WithMany(elem => elem.SpecificationExtensionComponents)
            .HasForeignKey(specExt => new { specExt.ExtensionComponentID, specExt.BusinessTermID })
            .HasPrincipalKey(elem => new { elem.ExtensionComponentID, elem.BusinessTermID })
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SpecificationCore>()
            .HasOne(sc => sc.CoreInvoiceModel)
            .WithMany(cim => cim.SpecificationCores)
            .HasForeignKey(sc => sc.BusinessTermID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SpecificationCore>()
            .HasOne(sc => sc.SpecificationIdentifyingInformation)
            .WithMany(sii => sii.SpecificationCores)
            .HasForeignKey(sc => sc.IdentityID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SpecificationExtensionComponent>()
            .HasOne(sec => sec.SpecificationIdentifyingInformation)
            .WithMany(sii => sii.SpecificationExtensionComponents)
            .HasForeignKey(sec => sec.IdentityID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExtensionComponentModelElement>()
            .HasOne(ece => ece.ExtensionComponentsModelHeader)
            .WithMany(ech => ech.ExtensionComponentModelElements)
            .HasForeignKey(ece => ece.ExtensionComponentID)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Configure New User and UserGroup Entities and Relationships ---

        // User Entity Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users"); // Explicit table name
            entity.HasKey(u => u.UserID);

            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Username).IsRequired().HasMaxLength(256);

            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);

            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
            entity.Property(u => u.IsActive).IsRequired();
            entity.Property(u => u.CreatedDate).IsRequired();

            // Relationship: User to UserGroup (One UserGroup can have many Users, a User belongs to one UserGroup)
            entity.HasOne(u => u.UserGroup)
                  .WithMany(g => g.Users) // Assumes UserGroup has an ICollection<User> Users
                  .HasForeignKey(u => u.UserGroupID)
                  .IsRequired(false) // UserGroupID is nullable in User model
                  .OnDelete(DeleteBehavior.Restrict); // Prevent UserGroup deletion if Users are assigned
        });

        // UserGroup Entity Configuration
        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.ToTable("UserGroups"); // Explicit table name
            entity.HasKey(g => g.UserGroupID);

            entity.HasIndex(g => g.GroupName).IsUnique();
            entity.Property(g => g.GroupName).IsRequired().HasMaxLength(100);
            entity.Property(g => g.CreatedDate).IsRequired();
        });

        // SpecificationIdentifyingInformation Entity Configuration (Relationship to UserGroup)
        modelBuilder.Entity<SpecificationIdentifyingInformation>(entity =>
        {
            // Relationship: SpecificationIdentifyingInformation to UserGroup
            // (One UserGroup can "own" many Specifications, a Specification is owned by one UserGroup)
            entity.HasOne(s => s.UserGroup)
                  .WithMany(g => g.Specifications) // Assumes UserGroup has an ICollection<SpecificationIdentifyingInformation> Specifications
                  .HasForeignKey(s => s.UserGroupID)
                  .IsRequired(true) // UserGroupID is required as foreign key in SpecificationIdentifyingInformation to ensure every specification has a governing entity
                  .OnDelete(DeleteBehavior.Restrict); // If UserGroup is deleted, set UserGroupID in Spec to NULL
        });

        modelBuilder.Entity<AdditionalRequirement>(entity =>
        {
            // Configure the composite primary key
            entity.HasKey(ar => new { ar.IdentityID, ar.BusinessTermID });

            // Configure the foreign key relationship to SpecificationIdentifyingInformation
            entity.HasOne(ar => ar.SpecificationIdentifyingInformation)
                  .WithMany(sii => sii.AdditionalRequirements) // Assumes you will add this navigation property to SpecificationIdentifyingInformation
                  .HasForeignKey(ar => ar.IdentityID)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // If you decide to implement the CreatorUserID for the User.CreatedSpecifications navigation property:
        // modelBuilder.Entity<SpecificationIdentifyingInformation>()
        //    .HasOne(s => s.CreatorUser) // Assuming CreatorUser navigation property in SpecInfo
        //    .WithMany(u => u.CreatedSpecifications) // In User model
        //    .HasForeignKey(s => s.CreatorUserID) // Assuming CreatorUserID FK in SpecInfo
        //    .IsRequired(false) // Or true, depending on requirements
        //    .OnDelete(DeleteBehavior.SetNull); // Or Restrict
    }
}
