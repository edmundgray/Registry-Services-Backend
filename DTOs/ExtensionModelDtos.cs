// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using System.ComponentModel.DataAnnotations;

namespace RegistryApi.DTOs;

/// <summary>
/// Data Transfer Object for ExtensionComponentsModelHeader.
/// </summary>
public record ExtensionComponentsModelHeaderDto(
    [Required][MaxLength(10)] string ID,
    [Required][MaxLength(100)] string Name,
    [MaxLength(50)] string? Status,
    string? ECLink
)
{
    // Parameterless constructor for AutoMapper
    public ExtensionComponentsModelHeaderDto() : this(string.Empty, string.Empty, null, null) { }
}

/// <summary>
/// Data Transfer Object for ExtensionComponentModelElement.
/// </summary>
public record ExtensionComponentModelElementDto(
    int EntityID,
    [Required][MaxLength(10)] string ExtensionComponentID,
    [Required][MaxLength(10)] string BusinessTermID,
    [Required][MaxLength(255)] string BusinessTerm,
    [MaxLength(10)] string? Level,
    [MaxLength(20)] string? Cardinality,
    string? SemanticDescription,
    string? UsageNoteCore,
    string? UsageNoteExtension,
    string? Justification,
    [MaxLength(50)] string? DataType,
    [MaxLength(50)] string? ExtensionType,
    [MaxLength(50)] string? ConformanceType,
    [MaxLength(50)] string? ParentID
)
{
    // Parameterless constructor for AutoMapper
    public ExtensionComponentModelElementDto() : this(0, string.Empty, string.Empty, string.Empty, null, null, null, null, null, null, null, null, null, null) { }
}

