// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using System.ComponentModel.DataAnnotations;

namespace RegistryApi.DTOs;

// Base record for common fields
public record SpecificationCoreBaseDto(
    [Required][MaxLength(10)] string BusinessTermID,
    [Required][MaxLength(20)] string Cardinality,
    string? UsageNote, // MaxLength(10) removed
    [Required] string TypeOfChange
);

// Create DTO inherits base fields
public record SpecificationCoreCreateDto(
    [Required][MaxLength(10)] string BusinessTermID,
    [Required][MaxLength(20)] string Cardinality,
    string? UsageNote, // MaxLength(10) removed
    [Required] string TypeOfChange
) : SpecificationCoreBaseDto(BusinessTermID, Cardinality, UsageNote, TypeOfChange);

// Update DTO inherits base fields
public record SpecificationCoreUpdateDto(
    [Required][MaxLength(10)] string BusinessTermID,
    [Required][MaxLength(20)] string Cardinality,
    string? UsageNote, // MaxLength(10) removed
    [Required] string TypeOfChange
) : SpecificationCoreBaseDto(BusinessTermID, Cardinality, UsageNote, TypeOfChange);

// DTO for retrieval includes the ID and new fields from CoreInvoiceModel
public record SpecificationCoreDto(
    int EntityID,
    int IdentityID, // Include parent ID for context
    [Required][MaxLength(10)] string BusinessTermID,
    [Required][MaxLength(20)] string Cardinality,
    string? UsageNote, // MaxLength(10) removed
    [Required] string TypeOfChange,
    string? CoreBusinessTerm,
    string? CoreLevel,
    string? CoreBusinessRules, // Added BusinessRules from CoreInvoiceModel
    string? CoreDataType, // Added DataType from CoreInvoiceModel
    string? CoreSemanticDescription,
    string? CoreParentID // NEW: Added ParentID from CoreInvoiceModel
) : SpecificationCoreBaseDto(BusinessTermID, Cardinality, UsageNote, TypeOfChange)
{
    // Parameterless constructor for AutoMapper
    // It calls the primary constructor with default values.
    // AutoMapper will then populate these properties with the actual mapped values.
    // It calls the primary constructor with default values for all parameters.
    public SpecificationCoreDto() : this(
        0,                            // EntityID
        0,                            // IdentityID
        string.Empty,                 // BusinessTermID
        string.Empty,                 // Cardinality
        null,                         // UsageNote
        string.Empty,                 // TypeOfChange
        null,                         // CoreLevel
        null,                         // CoreBusinessTerm
        null,                         // CoreBusinessRules
        null,                         // CoreDataType
        null,                         // CoreSemanticDescription
        null                          // CoreParentID // NEW: Added default for CoreParentID
    )
    { }
}

