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

    public string? GoverningEntity { get; set; }

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

    // Navigation properties - Initialized using collection expression []
    public virtual ICollection<SpecificationCore> SpecificationCores { get; set; } = [];
    public virtual ICollection<SpecificationExtensionComponent> SpecificationExtensionComponents { get; set; } = [];
}
