using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistryApi.Models;

[Table("ExtensionComponentModelElements")] // Keep original table name
public class ExtensionComponentModelElement
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EntityID { get; set; }

    [Required]
    [MaxLength(10)]
    public required string ExtensionComponentID { get; set; } // FK and part of composite key

    [Required]
    [MaxLength(10)]
    public required string BusinessTermID { get; set; } // Part of composite key

    [Required]
    [MaxLength(255)]
    public required string BusinessTerm { get; set; }

    [MaxLength(10)]
    public string? Level { get; set; }

    [MaxLength(20)]
    public string? Cardinality { get; set; }

    [Column(TypeName = "text")]
    public string? SemanticDescription { get; set; }

    // Add other properties as needed...
    [MaxLength(50)]
    public string? DataType { get; set; }
    [MaxLength(50)]
    public string? ExtensionType { get; set; }
    [MaxLength(50)]
    public string? ParentID { get; set; }


    // Navigation properties
    [ForeignKey("ExtensionComponentID")]
    public virtual required ExtensionComponentsModelHeader ExtensionComponentsModelHeader { get; set; }

    public virtual ICollection<SpecificationExtensionComponent> SpecificationExtensionComponents { get; set; } = [];
}
