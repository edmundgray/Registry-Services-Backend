// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using System.ComponentModel.DataAnnotations;

namespace RegistryApi.DTOs;

// Base record for common fields
public record SpecificationExtensionComponentBaseDto(
    [Required][MaxLength(10)] string ExtensionComponentID,
    [Required][MaxLength(10)] string BusinessTermID,
    [Required][MaxLength(20)] string Cardinality,
    string? UsageNote,
    string? Justification,
    [Required][MaxLength(50)] string TypeOfExtension
);

// Create DTO inherits base fields
public record SpecificationExtensionComponentCreateDto(
    [Required][MaxLength(10)] string ExtensionComponentID,
    [Required][MaxLength(10)] string BusinessTermID,
    [Required][MaxLength(20)] string Cardinality,
    string? UsageNote,
    string? Justification,
    [Required][MaxLength(50)] string TypeOfExtension
) : SpecificationExtensionComponentBaseDto(ExtensionComponentID, BusinessTermID, Cardinality, UsageNote, Justification, TypeOfExtension);

// Update DTO inherits base fields
public record SpecificationExtensionComponentUpdateDto(
    [Required][MaxLength(10)] string ExtensionComponentID,
    [Required][MaxLength(10)] string BusinessTermID,
    [Required][MaxLength(20)] string Cardinality,
    string? UsageNote,
    string? Justification,
    [Required][MaxLength(50)] string TypeOfExtension
) : SpecificationExtensionComponentBaseDto(ExtensionComponentID, BusinessTermID, Cardinality, UsageNote, Justification, TypeOfExtension);

// DTO for retrieval includes the ID and new fields from ExtensionComponentModelElement
public record SpecificationExtensionComponentDto(
    int EntityID,
    int IdentityID,
    [Required][MaxLength(10)] string ExtensionComponentID,
    [Required][MaxLength(10)] string BusinessTermID,
    [Required][MaxLength(20)] string Cardinality,
    string? UsageNote,
    string? Justification,
    [Required][MaxLength(50)] string TypeOfExtension,
    // New fields from ExtensionComponentModelElement
    string? ExtLevel,
    string? ExtBusinessTerm,
    string? ExtDataType,
    string? ExtConformanceType,
    string? ExtParentID // NEW: Added ParentID from ExtensionComponentModelElement
) : SpecificationExtensionComponentBaseDto(ExtensionComponentID, BusinessTermID, Cardinality, UsageNote, Justification, TypeOfExtension)
{
    // Parameterless constructor for AutoMapper needs to provide default values for ALL its primary constructor's parameters.
    public SpecificationExtensionComponentDto() : this(
       0,                            // EntityID
       0,                            // IdentityID
       string.Empty,                 // ExtensionComponentID
       string.Empty,                 // BusinessTermID
       string.Empty,                 // Cardinality
       null,                         // UsageNote
       null,                         // Justification
       string.Empty,                 // TypeOfExtension
       null,                         // ExtLevel
       null,                         // ExtBusinessTerm
       null,                         // ExtDataType
       null,                         // ExtConformanceType
       null                          // ExtParentID // NEW: Added default for ExtParentID
   )
    { }
}

