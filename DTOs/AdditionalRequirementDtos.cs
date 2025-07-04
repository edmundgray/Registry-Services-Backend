using System.ComponentModel.DataAnnotations;

namespace RegistryApi.DTOs
{
    // DTO for creating a new Additional Requirement
    public record AdditionalRequirementCreateDto(
        [Required][MaxLength(10)] string BusinessTermID,
        [Required][MaxLength(255)] string BusinessTermName,
        [Required][MaxLength(10)] string Level,
        [Required][MaxLength(20)] string Cardinality,
        short RowPos,
        string? SemanticDescription,
        string? UsageNote,
        [MaxLength(50)] string? DataType,
        [MaxLength(50)] string? BusinessRules,
        [Required][MaxLength(50)] string TypeOfChange
    );

    // DTO for updating an existing Additional Requirement
    public record AdditionalRequirementUpdateDto(
        [Required][MaxLength(255)] string BusinessTermName,
        [Required][MaxLength(10)] string Level,
        [Required][MaxLength(20)] string Cardinality,
        short RowPos,
        string? SemanticDescription,
        string? UsageNote,
        [MaxLength(50)] string? DataType,
        [MaxLength(50)] string? BusinessRules,
        [Required][MaxLength(50)] string TypeOfChange
    );

    // DTO for retrieving an Additional Requirement
    public record AdditionalRequirementDto(
        string BusinessTermID,
        int IdentityID,
        string BusinessTermName,
        string Level,
        string Cardinality,
        short RowPos,
        string? SemanticDescription,
        string? UsageNote,
        string? DataType,
        string? BusinessRules,
        string TypeOfChange
    );
}