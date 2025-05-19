using System.ComponentModel.DataAnnotations;

namespace RegistryApi.DTOs;

// Base record for common fields
public record SpecificationCoreBaseDto(
    [Required][MaxLength(10)] string BusinessTermID,
    [Required][MaxLength(20)] string Cardinality,
    [MaxLength(10)] string? UsageNote,
    [Required] string TypeOfChange
);

// Create DTO inherits base fields
public record SpecificationCoreCreateDto(
    [Required][MaxLength(10)] string BusinessTermID,
    [Required][MaxLength(20)] string Cardinality,
    [MaxLength(10)] string? UsageNote,
    [Required] string TypeOfChange
) : SpecificationCoreBaseDto(BusinessTermID, Cardinality, UsageNote, TypeOfChange);

// Update DTO inherits base fields
public record SpecificationCoreUpdateDto(
    [Required][MaxLength(10)] string BusinessTermID,
    [Required][MaxLength(20)] string Cardinality,
    [MaxLength(10)] string? UsageNote,
    [Required] string TypeOfChange
) : SpecificationCoreBaseDto(BusinessTermID, Cardinality, UsageNote, TypeOfChange);

// DTO for retrieval includes the ID and new fields from CoreInvoiceModel
public record SpecificationCoreDto(
    int EntityID,
    int IdentityID, // Include parent ID for context
    [Required][MaxLength(10)] string BusinessTermID,
    [Required][MaxLength(20)] string Cardinality,
    [MaxLength(10)] string? UsageNote,
    [Required] string TypeOfChange,
    // New fields from CoreInvoiceModel
    string? CoreBusinessTerm,
    string? CoreLevel,
    string? CoreSemanticDescription
) : SpecificationCoreBaseDto(BusinessTermID, Cardinality, UsageNote, TypeOfChange)
{
    // Parameterless constructor for AutoMapper
    // It calls the primary constructor with default values.
    // AutoMapper will then populate these properties with the actual mapped values.
    public SpecificationCoreDto() : this(0, 0, string.Empty, string.Empty, null, string.Empty, null, null, null) { }
}

// Response for paginated list
public record PaginatedSpecificationCoreResponse(
    PaginationMetadata Metadata,
    List<SpecificationCoreDto> Items
);