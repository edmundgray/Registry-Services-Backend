// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using System.ComponentModel.DataAnnotations;

namespace RegistryApi.DTOs;

/// <summary>
/// Data Transfer Object for CoreInvoiceModel.
/// </summary>
public record CoreInvoiceModelDto(
    [Required][MaxLength(10)] string ID,
    [Required][MaxLength(255)] string BusinessTerm,
    [Required][MaxLength(10)] string Level,
    [Required][MaxLength(20)] string Cardinality,
    short RowPos,
    string? SemanticDescription,
    string? UsageNote,
    string? DataType,
    string? BusinessRules
)
{
    /// <summary>
    /// Parameterless constructor for model binding and serialization.
    /// </summary>
    /// <remarks>
    /// Initializes the DTO with default values. These will be overwritten by AutoMapper or other mapping mechanisms.
    /// </remarks>
    public CoreInvoiceModelDto() : this(string.Empty, string.Empty, string.Empty, string.Empty, 0, null, null, null, null) { }
}