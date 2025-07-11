using System;
using System.ComponentModel.DataAnnotations;

namespace RegistryApi.DTOs
{
    // For creating a new user
    public record UserCreateDto(
        [Required][MaxLength(255)] string Username,
        [Required][MinLength(8)] string Password, // Basic password length validation
        [Required][MaxLength(320)][EmailAddress] string Email,
        [MaxLength(100)] string? FirstName,
        [MaxLength(100)] string? LastName,
        [Required] string Role, // e.g., "Admin", "User" - validation might be needed in service
        int? UserGroupID // Nullable if user is not immediately assigned to a group or is Admin
    );

    // For updating an existing user (Admin typically does this)
    public record UserUpdateDto(
        [MaxLength(100)] string? FirstName,
        [MaxLength(100)] string? LastName,
        [Required][MaxLength(320)][EmailAddress] string Email,
        [Required] string Role,
        int? UserGroupID, // Allow changing or unsetting group
        bool? IsActive
    );

    // For returning user details (excluding password)
    public record UserDto(
        int UserID,
        string Username,
        string Email,
        string? FirstName,
        string? LastName,
        string Role,
        int? UserGroupID,
        string? GroupName, // Display name of the group
        bool IsActive,
        DateTime CreatedDate,
        DateTime? LastLoginDate
    )
    {
        // Parameterless constructor for AutoMapper
        public UserDto() : this(0, string.Empty, string.Empty, null, null, string.Empty, null, null, false, DateTime.MinValue, null) { }
    }

    // For user login
    public record UserLoginDto(
        [Required] string Username,
        [Required] string Password
    );

    // For returning a token (and basic user info) after successful login
    public record UserTokenDto(
        string token,
        string refreshToken,
        long expiresIn,
        int userId,
        string username,
        string role,
        int? userGroupID,
        string? groupName
    );

    public record RoleChangeDto(
        [Required(ErrorMessage = "NewRole is required.")] // Added validation
        string NewRole
    );
    // Could also have a Dto for password change, etc.

    public record RefreshTokenDto(
    [Required] string refreshToken,
    [Required] string accessToken
    );
}