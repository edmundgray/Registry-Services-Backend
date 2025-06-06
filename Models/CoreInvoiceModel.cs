using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistryApi.Models;

[Table("CoreInvoiceModel")]
public class CoreInvoiceModel
{
    [Key]
    [MaxLength(10)]
    public required string ID { get; set; } // BusinessTermID in SpecificationCore FK

    [Required]
    [MaxLength(255)]
    public required string BusinessTerm { get; set; }

    [Required]
    [MaxLength(10)]
    public required string Level { get; set; }

    [Required]
    [MaxLength(20)]
    public required string Cardinality { get; set; }

    /// <summary>
    /// Position of the row for maintaining juxtaposition, crucial for ordered display.
    /// </summary>
    public short RowPos { get; set; }

    // Add other properties as needed...
    [Column(TypeName = "text")]
    public string? SemanticDescription { get; set; }
    [MaxLength(50)]
    public string? DataType { get; set; }
    [MaxLength(50)]
    public string? BusinessRules { get; set; }
    [MaxLength(50)]
    public string? ParentID { get; set; }

    // Navigation property
    public virtual ICollection<SpecificationCore> SpecificationCores { get; set; } = [];
}
