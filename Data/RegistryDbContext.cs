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


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Configure Composite Keys and Relationships ---

        // Unique constraint on ExtensionComponentModelElement (acts as Principal Key for the composite FK)
        modelBuilder.Entity<ExtensionComponentModelElement>()
            .HasIndex(e => new { e.ExtensionComponentID, e.BusinessTermID })
            .IsUnique()
            .HasDatabaseName("UQ_ExtensionComponentModelElements_Component_BusinessTerm"); // Match SQL name

        // Composite foreign key from SpecificationExtensionComponent to ExtensionComponentModelElement
        modelBuilder.Entity<SpecificationExtensionComponent>()
            .HasOne(specExt => specExt.ExtensionComponentModelElement)
            .WithMany(elem => elem.SpecificationExtensionComponents)
            .HasForeignKey(specExt => new { specExt.ExtensionComponentID, specExt.BusinessTermID }) // Dependent side FK properties
            .HasPrincipalKey(elem => new { elem.ExtensionComponentID, elem.BusinessTermID }) // Principal side PK properties
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting ExtensionComponentModelElement if used

         // FK from SpecificationCore to CoreInvoiceModel
        modelBuilder.Entity<SpecificationCore>()
            .HasOne(sc => sc.CoreInvoiceModel)
            .WithMany(cim => cim.SpecificationCores)
            .HasForeignKey(sc => sc.BusinessTermID)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting CoreInvoiceModel if used

        // FK from SpecificationCore to SpecificationIdentifyingInformation
        modelBuilder.Entity<SpecificationCore>()
            .HasOne(sc => sc.SpecificationIdentifyingInformation)
            .WithMany(sii => sii.SpecificationCores)
            .HasForeignKey(sc => sc.IdentityID)
            .OnDelete(DeleteBehavior.Cascade); // Deleting Spec Header deletes its Core items

        // FK from SpecificationExtensionComponent to SpecificationIdentifyingInformation
        modelBuilder.Entity<SpecificationExtensionComponent>()
            .HasOne(sec => sec.SpecificationIdentifyingInformation)
            .WithMany(sii => sii.SpecificationExtensionComponents)
            .HasForeignKey(sec => sec.IdentityID)
            .OnDelete(DeleteBehavior.Cascade); // Deleting Spec Header deletes its Extension items

        // FK from ExtensionComponentModelElement to ExtensionComponentsModelHeader
        modelBuilder.Entity<ExtensionComponentModelElement>()
            .HasOne(ece => ece.ExtensionComponentsModelHeader)
            .WithMany(ech => ech.ExtensionComponentModelElements)
            .HasForeignKey(ece => ece.ExtensionComponentID)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting header if elements exist
    }
}
