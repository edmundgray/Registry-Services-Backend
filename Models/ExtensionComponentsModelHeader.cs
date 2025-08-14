// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistryApi.Models;

[Table("ExtensionComponentsModelHeader")]
public class ExtensionComponentsModelHeader
{
    [Key]
    [MaxLength(10)]
    public required string ID { get; set; } // ExtensionComponentID in ExtensionComponentModelElement FK

    [Required]
    [MaxLength(100)] // Based on SQL script nchar(100)
    public required string Name { get; set; }

    [MaxLength(50)] // Based on SQL script nchar(50)
    public string? Status { get; set; }

    public string? ECLink { get; set; } // nvarchar(max) -> string?

    // Navigation property
    public virtual ICollection<ExtensionComponentModelElement> ExtensionComponentModelElements { get; set; } = [];
}
