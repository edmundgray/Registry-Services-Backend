
// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistryApi.Models
{
    [Table("Users")] // Explicitly naming the table
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Username { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        [Required]
        [MaxLength(320)]
        public required string Email { get; set; }

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Role { get; set; } // "Admin" or "User"

        public int? UserGroupID { get; set; } // Foreign Key

        [Required]
        public bool IsActive { get; set; } = true; // Default to true

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation Properties
        [ForeignKey("UserGroupID")]
        public virtual UserGroup? UserGroup { get; set; }

        // This navigation property implies a one-to-many relationship
        // where a User can create multiple Specifications.
        // For this to be fully mapped by EF Core, SpecificationIdentifyingInformation
        // would need a corresponding CreatorUserID foreign key and a User navigation property.
        // The current plan focuses on UserGroupID for ownership/editing permissions.
        public virtual ICollection<SpecificationIdentifyingInformation> CreatedSpecifications { get; set; } = new List<SpecificationIdentifyingInformation>();
    }
}