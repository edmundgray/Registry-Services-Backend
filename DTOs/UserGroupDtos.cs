using System;
using System.ComponentModel.DataAnnotations;

namespace RegistryApi.DTOs
{
    public record UserGroupCreateDto(
        [Required][MaxLength(100)] string GroupName,
        [MaxLength(500)] string? Description
    );

    public record UserGroupUpdateDto(
        [Required][MaxLength(100)] string GroupName,
        [MaxLength(500)] string? Description
    );

    public record UserGroupDto(
        int UserGroupID,
        string GroupName,
        string? Description,
        DateTime CreatedDate,
        // New statistical fields
        int NumberUsers,
        int NumberSpecifications,
        int NumberInProgress,
        int NumberSubmitted,
        int NumberUnderReview,
        int NumberVerified
    );

    // Could also have UserGroupWithUsersDto if needed
}


