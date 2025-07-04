using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistryApi.Models
{
    [Table("AdditionalRequirements")]
    public class AdditionalRequirement
    {
        [Key]
        [Column(Order = 0)]
        [MaxLength(10)]
        public required string BusinessTermID { get; set; }

        [Key]
        [Column(Order = 1)]
        public int IdentityID { get; set; }

        [Required]
        [MaxLength(255)]
        public required string BusinessTermName { get; set; }

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

        [Column(TypeName = "text")]
        public string? SemanticDescription { get; set; }

        [Column(TypeName = "text")]
        public string? UsageNote { get; set; }

        [MaxLength(50)]
        public string? DataType { get; set; }

        [MaxLength(50)]
        public string? BusinessRules { get; set; }

        [Required]
        [MaxLength(50)]
        public required string TypeOfChange { get; set; }

        // Navigation properties
        [ForeignKey("IdentityID")]
        public virtual required SpecificationIdentifyingInformation SpecificationIdentifyingInformation { get; set; }
    }
}