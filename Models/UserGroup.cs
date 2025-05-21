using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistryApi.Models
{
    [Table("UserGroups")] // Explicitly naming the table
    public class UserGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserGroupID { get; set; }

        [Required]
        [MaxLength(100)]
        public required string GroupName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        // Navigation Properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<SpecificationIdentifyingInformation> Specifications { get; set; } = new List<SpecificationIdentifyingInformation>();
    }
}
