using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistryApi.Models;

[Table("SpecificationCore")]
public class SpecificationCore
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EntityID { get; set; }

    [Required]
    public int IdentityID { get; set; } // FK to SpecificationIdentifyingInformation

    [Required]
    [MaxLength(10)]
    public required string BusinessTermID { get; set; } // FK to CoreInvoiceModel

    [Required]
    [MaxLength(20)]
    public required string Cardinality { get; set; }

    [Column(TypeName = "text")]
    public string? UsageNote { get; set; }

    [Required]
    [MaxLength(50)]
    public required string TypeOfChange { get; set; }

    // Navigation properties
    [ForeignKey("IdentityID")]
    public virtual required SpecificationIdentifyingInformation SpecificationIdentifyingInformation { get; set; }

    [ForeignKey("BusinessTermID")]
    public virtual required CoreInvoiceModel CoreInvoiceModel { get; set; }
}
