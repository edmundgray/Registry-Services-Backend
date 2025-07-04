using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistryApi.Models;

[Table("SpecificationIdentifyingInformation")]
public class SpecificationIdentifyingInformation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdentityID { get; set; }

    [Required]
    [MaxLength(255)]
    public required string SpecificationIdentifier { get; set; }

    [Required]
    public required string SpecificationName { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Sector { get; set; }

    [MaxLength(200)]
    public string? SubSector { get; set; }

    [Required]
    public required string Purpose { get; set; }

    [MaxLength(50)]
    public string? SpecificationVersion { get; set; }

    [Required]
    public required string ContactInformation { get; set; }

    public DateTime? DateOfImplementation { get; set; }
    // This property is REMOVED and replaced with UserGroupID
    // public string? GoverningEntity { get; set; }
    [Required]
    public int UserGroupID { get; set; } // non Nullable Foreign Key to UserGroup where groupName = GoverningEntity

    [MaxLength(50)]
    public string? CoreVersion { get; set; }

    [MaxLength(255)]
    public string? SpecificationSourceLink { get; set; }

    [MaxLength(200)]
    public string? Country { get; set; }

    public bool IsCountrySpecification { get; set; }

    [MaxLength(255)]
    public string? UnderlyingSpecificationIdentifier { get; set; }

    [MaxLength(100)]
    public string? PreferredSyntax { get; set; }

    /// <summary>
    /// The level of conformance for the specification.
    /// </summary>
    [MaxLength(25)]
    public string? ConformanceLevel { get; set; }

    // New audit fields
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    
   

    // New Status Fields
    [MaxLength(15)] // Adjust MaxLength as appropriate
    public string? ImplementationStatus { get; set; } // e.g., Planned, Development, Active, Revoked

    [MaxLength(15)] // Adjust MaxLength as appropriate
    public string? RegistrationStatus { get; set; } // e.g., Submitted, Under review, Verified

    /// <summary>
    /// Type of the specification (e.g., "Core", "Extension", "CountrySpecific").
    /// </summary>
    [MaxLength(50)]
    public string? SpecificationType { get; set; }

    // New Navigation properties
    [ForeignKey("UserGroupID")]
    public virtual UserGroup? UserGroup { get; set; }


    // Navigation properties - Initialized using collection expression []
    public virtual ICollection<SpecificationCore> SpecificationCores { get; set; } = [];
    public virtual ICollection<SpecificationExtensionComponent> SpecificationExtensionComponents { get; set; } = [];
    
    public virtual ICollection<AdditionalRequirement> AdditionalRequirements { get; set; } = [];
}
