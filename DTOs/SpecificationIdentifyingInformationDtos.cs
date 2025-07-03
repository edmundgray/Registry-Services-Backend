using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryApi.DTOs;

public record SpecificationIdentifyingInformationCreateDto(
    [Required][MaxLength(255)] string SpecificationIdentifier,
    [Required] string SpecificationName,
    [Required][MaxLength(200)] string Sector,
    [MaxLength(200)] string? SubSector,
    [Required] string Purpose,
    [MaxLength(50)] string? SpecificationVersion,
    [Required] string ContactInformation,
    DateTime? DateOfImplementation,
    [Required] int UserGroupID,
    [MaxLength(50)] string? CoreVersion,
    [MaxLength(255)] string? SpecificationSourceLink,
    [MaxLength(200)] string? Country,
    [MaxLength(50)] string? SpecificationType = "Extension",
    bool IsCountrySpecification = false,
    [MaxLength(255)] string? UnderlyingSpecificationIdentifier = null,
    [MaxLength(100)] string? PreferredSyntax = null,
    [MaxLength(15)] string? ImplementationStatus = "Planned",
    [MaxLength(15)] string? RegistrationStatus = "Submitted",
    [MaxLength(25)] string? ConformanceLevel = "Core Conformant"
);

public record SpecificationIdentifyingInformationUpdateDto(
    [Required][MaxLength(255)] string SpecificationIdentifier,
    [Required] string SpecificationName,
    [Required][MaxLength(200)] string Sector,
    [MaxLength(200)] string? SubSector,
    [Required] string Purpose,
    [MaxLength(50)] string? SpecificationVersion,
    [Required] string ContactInformation,
    DateTime? DateOfImplementation,
    [Required] int UserGroupID,
    [MaxLength(50)] string? CoreVersion,
    [MaxLength(255)] string? SpecificationSourceLink,
    [MaxLength(200)] string? Country,
    bool IsCountrySpecification,
    [MaxLength(255)] string? UnderlyingSpecificationIdentifier,
    [MaxLength(100)] string? PreferredSyntax,
    [MaxLength(15)] string? ImplementationStatus,
    [MaxLength(15)] string? RegistrationStatus,
    [MaxLength(50)] string? SpecificationType,
    [MaxLength(25)] string? ConformanceLevel
);

public record SpecificationIdentifyingInformationHeaderDto(
    int IdentityID,
    string SpecificationIdentifier,
    string SpecificationName,
    string Sector,
    string? SpecificationVersion,
    DateTime? DateOfImplementation,
    string? Country,
    DateTime CreatedDate,
    DateTime ModifiedDate,
    string? ImplementationStatus,
    string? RegistrationStatus,
    string? SpecificationType,
    string? ConformanceLevel,
    string? Purpose,
    string? PreferredSyntax,
    string? GoverningEntity, // This is now UserGroupID in the model, the group name is used as GoverningEntity
    int UserGroupID
)
{
    public SpecificationIdentifyingInformationHeaderDto() : this(0, string.Empty, string.Empty, string.Empty, null, null, null, DateTime.MinValue, DateTime.MinValue, default!, default!, default!, default!, default!, default!, default!, 0) { }
}

public record PaginatedSpecificationHeaderResponse(
    PaginationMetadata Metadata,
    List<SpecificationIdentifyingInformationHeaderDto> Items
);

public record SpecificationIdentifyingInformationDetailDto(
    int IdentityID,
    string SpecificationIdentifier,
    string SpecificationName,
    string Sector,
    string? SpecificationVersion,
    DateTime? DateOfImplementation,
    string? Country,
    string? SubSector,
    string Purpose,
    string ContactInformation,
    string? GoverningEntity,
    int UserGroupID,
    string? CoreVersion,
    string? SpecificationSourceLink,
    bool IsCountrySpecification,
    string? UnderlyingSpecificationIdentifier,
    string? PreferredSyntax,
    DateTime CreatedDate,
    DateTime ModifiedDate,
    string? ImplementationStatus,
    string? RegistrationStatus,
    string? SpecificationType,
    string? ConformanceLevel,
    ICollection<SpecificationCoreDto> SpecificationCores,
    ICollection<SpecificationExtensionComponentDto> SpecificationExtensionComponents
)
{
    public SpecificationIdentifyingInformationDetailDto() : this(
        0, string.Empty, string.Empty, string.Empty, null, null, null, null, string.Empty, string.Empty,
        null, 0, null, null, false, null, null, DateTime.MinValue, DateTime.MinValue,
        null, null, null, null,
        default!, default!
    )
    { }
}