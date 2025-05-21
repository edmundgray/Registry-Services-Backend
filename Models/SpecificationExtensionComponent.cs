using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistryApi.Models;

[Table("SpecificationExtensionComponents")] // Keep original table name
public class SpecificationExtensionComponent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EntityID { get; set; }

    [Required]
    public int IdentityID { get; set; } // FK to SpecificationIdentifyingInformation

    [Required] // Made required based on report/script modification
    [MaxLength(10)]
    public required string ExtensionComponentID { get; set; } // Part of composite FK

    [Required]
    [MaxLength(10)]
    public required string BusinessTermID { get; set; } // Part of composite FK

    [Required]
    [MaxLength(20)]
    public required string Cardinality { get; set; }

    [Column(TypeName = "text")]
    public string? UsageNote { get; set; }

    [Column(TypeName = "text")]
    public string? Justification { get; set; }

    [Required]
    [MaxLength(50)]
    public required string TypeOfExtension { get; set; }

    // Navigation properties
    [ForeignKey("IdentityID")]
    public virtual required SpecificationIdentifyingInformation SpecificationIdentifyingInformation { get; set; }

    // Navigation property for the composite foreign key
    public virtual required ExtensionComponentModelElement ExtensionComponentModelElement { get; set; }
}

