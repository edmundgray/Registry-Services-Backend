using System; // Required for DateTime
using System.Collections.Generic; // Required for List
using System.ComponentModel.DataAnnotations;

namespace RegistryApi.DTOs;

// Using records for DTOs where appropriate (especially for immutability)

public record SpecificationIdentifyingInformationCreateDto(
    [Required][MaxLength(255)] string SpecificationIdentifier,
    [Required] string SpecificationName,
    [Required][MaxLength(200)] string Sector,
    [MaxLength(200)] string? SubSector,
    [Required] string Purpose,
    [MaxLength(50)] string? SpecificationVersion,
    [Required] string ContactInformation,
    DateTime? DateOfImplementation,
    string? GoverningEntity,
    [MaxLength(50)] string? CoreVersion,
    [MaxLength(255)] string? SpecificationSourceLink,
    [MaxLength(200)] string? Country,
    bool IsCountrySpecification = false, // Default value
    [MaxLength(255)] string? UnderlyingSpecificationIdentifier = null,
    [MaxLength(100)] string? PreferredSyntax = null
// CreatedDate and ModifiedDate are not included here as they are server-set
);

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
    [MaxLength(50)] string? CoreVersion,
    [MaxLength(255)] string? SpecificationSourceLink,
    [MaxLength(200)] string? Country,
    bool IsCountrySpecification,
    [MaxLength(255)] string? UnderlyingSpecificationIdentifier,
    [MaxLength(100)] string? PreferredSyntax
// CreatedDate and ModifiedDate are not included here as they are server-set
);


public record SpecificationIdentifyingInformationHeaderDto(
    int IdentityID,
    string SpecificationIdentifier,
    string SpecificationName,
    string Sector,
    string? SpecificationVersion,
    DateTime? DateOfImplementation,
    string? Country,
    // New audit fields
    DateTime CreatedDate,
    DateTime ModifiedDate
)
{
    // Parameterless constructor for AutoMapper if ever needed, though less likely for this specific DTO
    public SpecificationIdentifyingInformationHeaderDto() : this(0, string.Empty, string.Empty, string.Empty, null, null, null, DateTime.MinValue, DateTime.MinValue) { }
}

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
    // New audit fields
    DateTime CreatedDate,
    DateTime ModifiedDate,
    // Paginated child lists
    PaginatedSpecificationCoreResponse SpecificationCores,
    PaginatedSpecificationExtensionResponse SpecificationExtensionComponents
)
{
    // Parameterless constructor for AutoMapper
    public SpecificationIdentifyingInformationDetailDto() : this(
        0, // IdentityID
        string.Empty, // SpecificationIdentifier
        string.Empty, // SpecificationName
        string.Empty, // Sector
        null, // SpecificationVersion
        null, // DateOfImplementation
        null, // Country
        null, // SubSector
        string.Empty, // Purpose
        string.Empty, // ContactInformation
        null, // GoverningEntity
        null, // CoreVersion
        null, // SpecificationSourceLink
        false, // IsCountrySpecification
        null, // UnderlyingSpecificationIdentifier
        null, // PreferredSyntax
        DateTime.MinValue, // CreatedDate
        DateTime.MinValue, // ModifiedDate
        default!, // SpecificationCores (will be null)
        default!  // SpecificationExtensionComponents (will be null)
    )
    { }
}
