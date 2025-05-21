using System.ComponentModel.DataAnnotations;

namespace RegistryApi.DTOs;

// Base record for common fields
public record SpecificationExtensionComponentBaseDto(
    [Required] [MaxLength(10)] string ExtensionComponentID,
    [Required] [MaxLength(10)] string BusinessTermID,
    [Required] [MaxLength(20)] string Cardinality,
    string? UsageNote,
    string? Justification,
    [Required] [MaxLength(50)] string TypeOfExtension
);

// Create DTO inherits base fields
public record SpecificationExtensionComponentCreateDto(
    [Required] [MaxLength(10)] string ExtensionComponentID,
    [Required] [MaxLength(10)] string BusinessTermID,
    [Required] [MaxLength(20)] string Cardinality,
    string? UsageNote,
    string? Justification,
    [Required] [MaxLength(50)] string TypeOfExtension
) : SpecificationExtensionComponentBaseDto(ExtensionComponentID, BusinessTermID, Cardinality, UsageNote, Justification, TypeOfExtension);

// Update DTO inherits base fields
public record SpecificationExtensionComponentUpdateDto(
    [Required] [MaxLength(10)] string ExtensionComponentID,
    [Required] [MaxLength(10)] string BusinessTermID,
    [Required] [MaxLength(20)] string Cardinality,
    string? UsageNote,
    string? Justification,
    [Required] [MaxLength(50)] string TypeOfExtension
) : SpecificationExtensionComponentBaseDto(ExtensionComponentID, BusinessTermID, Cardinality, UsageNote, Justification, TypeOfExtension);

// DTO for retrieval includes the ID
public record SpecificationExtensionComponentDto(
    int EntityID,
    int IdentityID,
    [Required] [MaxLength(10)] string ExtensionComponentID,
    [Required] [MaxLength(10)] string BusinessTermID,
    [Required] [MaxLength(20)] string Cardinality,
    string? UsageNote,
    string? Justification,
    [Required] [MaxLength(50)] string TypeOfExtension
) : SpecificationExtensionComponentBaseDto(ExtensionComponentID, BusinessTermID, Cardinality, UsageNote, Justification, TypeOfExtension);

// Response for paginated list
public record PaginatedSpecificationExtensionResponse(
    PaginationMetadata Metadata,
    List<SpecificationExtensionComponentDto> Items
);
