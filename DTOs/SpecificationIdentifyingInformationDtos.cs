using System.ComponentModel.DataAnnotations;

namespace RegistryApi.DTOs;

// Using records for DTOs where appropriate (especially for immutability)

public record SpecificationIdentifyingInformationCreateDto
{
    [Required]
    [MaxLength(255)]
    public string SpecificationIdentifier { get; init; }

    [Required]
    public string SpecificationName { get; init; }

    [Required]
    [MaxLength(200)]
    public string Sector { get; init; }

    [MaxLength(200)]
    public string? SubSector { get; init; }

    [Required]
    public string Purpose { get; init; }

    [MaxLength(50)]
    public string? SpecificationVersion { get; init; }

    [Required]
    public string ContactInformation { get; init; }

    public DateTime? DateOfImplementation { get; init; }

    public string? GoverningEntity { get; init; }

    [MaxLength(50)]
    public string? CoreVersion { get; init; }

    [MaxLength(255)]
    public string? SpecificationSourceLink { get; init; }

    [MaxLength(200)]
    public string? Country { get; init; }

    [MaxLength(255)]
    public string? UnderlyingSpecificationIdentifier { get; init; } = null;

    [MaxLength(100)]
    public string? PreferredSyntax { get; init; } = null;

    [MaxLength(50)]
    public string? SpecificationType { get; init; } = null;

    [MaxLength(50)]
    public string? RegistryStatus { get; init; } = null;

    [MaxLength(50)]
    public string? ConformanceLevel { get; init; } = null;

    public bool IsCountrySpecification { get; init; } = false;
}

// Update DTO often mirrors Create DTO for PUT operations
public record SpecificationIdentifyingInformationUpdateDto(
    [Required][MaxLength(255)] string SpecificationIdentifier,
    [Required] string SpecificationName,
    [Required][MaxLength(200)] string Sector,
    [MaxLength(200)] string? SubSector,
    [Required] string Purpose,
    [MaxLength(50)] string? SpecificationVersion,
    [Required] string ContactInformation,
    DateTime? DateOfImplementation,
    string? GoverningEntity,
    bool IsCountrySpecification,
    [MaxLength(50)] string? CoreVersion,
    [MaxLength(255)] string? SpecificationSourceLink,
    [MaxLength(200)] string? Country,
    [MaxLength(255)] string? UnderlyingSpecificationIdentifier = null,
    [MaxLength(100)] string? PreferredSyntax = null,
    [MaxLength(50)] string? SpecificationType = null,
    [MaxLength(50)] string? RegistryStatus = null,
    [MaxLength(50)] string? ConformanceLevel = null
    
);


public record SpecificationIdentifyingInformationHeaderDto(
    int IdentityID,
    string SpecificationIdentifier,
    string SpecificationName,
    string Sector,
    string? SpecificationVersion,
    DateTime? DateOfImplementation,
    string? Country
);

// Response for paginated list of headers
public record PaginatedSpecificationHeaderResponse(
    PaginationMetadata Metadata,
    List<SpecificationIdentifyingInformationHeaderDto> Items
);

// Detail DTO includes all fields and paginated child lists
public record SpecificationIdentifyingInformationDetailDto(
    // Inherited fields conceptually from HeaderDto
    int IdentityID,
    string SpecificationIdentifier,
    string SpecificationName,
    string Sector,
    string? SpecificationVersion,
    DateTime? DateOfImplementation,
    string? Country,
    // Additional fields
    string? SubSector,
    string Purpose,
    string ContactInformation,
    string? GoverningEntity,
    string? CoreVersion,
    string? SpecificationSourceLink,
    bool IsCountrySpecification,

    string? UnderlyingSpecificationIdentifier,
    string? PreferredSyntax,
   
    // Paginated child lists
    PaginatedSpecificationCoreResponse SpecificationCores,
    PaginatedSpecificationExtensionResponse SpecificationExtensionComponents
     
       
   
);

