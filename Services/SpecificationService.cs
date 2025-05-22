using AutoMapper;
using RegistryApi.DTOs;
using RegistryApi.Models;
using RegistryApi.Repositories;
using RegistryApi.Helpers; // For PagedList
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // For logging
using System; // For DateTime
using System.Collections.Generic; // For List

namespace RegistryApi.Services;

// Define a simple context for the current user - this will be replaced by HttpContext.User later
public record CurrentUserContext(int UserId, string Role, int? UserGroupId);


public class SpecificationService(
    ISpecificationIdentifyingInformationRepository specInfoRepo,
    ISpecificationCoreRepository specCoreRepo,
    ISpecificationExtensionComponentRepository specExtRepo,
    IMapper mapper,
    ILogger<SpecificationService> logger // Inject Logger
    ) : ISpecificationService
{
    // --- Helper method for permission checks (basic version) ---
    private async Task<bool> CanUserEditSpecification(int specificationId, CurrentUserContext? currentUser)
    {
        if (currentUser == null) return false; // No user context, no permission
        if (currentUser.Role == "Admin") return true; // Admins can edit anything

        var spec = await specInfoRepo.GetByIdAsync(specificationId);
        if (spec == null) return false; // Specification not found

        // Users can edit if their group owns the specification
        return spec.UserGroupID.HasValue && spec.UserGroupID == currentUser.UserGroupId;
    }


    // --- Specification Header Methods ---

    public async Task<PaginatedSpecificationHeaderResponse> GetSpecificationsAsync(PaginationParams paginationParams)
    {
        var pagedEntities = await specInfoRepo.GetAllPaginatedAsync(paginationParams);
        var dtos = mapper.Map<List<SpecificationIdentifyingInformationHeaderDto>>(pagedEntities.Items);

        return new PaginatedSpecificationHeaderResponse(
            Metadata: new PaginationMetadata(
                pagedEntities.TotalCount,
                pagedEntities.PageSize,
                pagedEntities.PageNumber,
                pagedEntities.TotalPages,
                pagedEntities.HasNextPage,
                pagedEntities.HasPreviousPage
            ),
            Items: dtos
        );
    }

    public async Task<(ServiceResult Status, PaginatedSpecificationHeaderResponse? Response)> GetSpecificationsByUserGroupAsync(
        CurrentUserContext currentUser,
        PaginationParams paginationParams)
    {
        PagedList<SpecificationIdentifyingInformation> pagedEntities;

        if (currentUser == null)
        {
            logger.LogWarning("Attempt to get specifications by user group without user context.");
            return (ServiceResult.Unauthorized, null);
        }

        if (currentUser.Role == "Admin")
        {
            // Admins see all specifications
            pagedEntities = await specInfoRepo.GetAllPaginatedAsync(paginationParams);
        }
        else if (currentUser.Role == "User")
        {
            if (!currentUser.UserGroupId.HasValue)
            {
                // User has no group, so they are forbidden from seeing group-specific specifications
                logger.LogWarning("User {UserId} attempted to get group specifications but has no UserGroupID.", currentUser.UserId);
                return (ServiceResult.Forbidden, null);
            }
            // Fetch specifications for the user's specific group
            pagedEntities = await specInfoRepo.GetByUserGroupIdPaginatedAsync(currentUser.UserGroupId.Value, paginationParams);
        }
        else
        {
            logger.LogError("Unknown role {Role} for user {UserId} attempting to get group specifications.", currentUser.Role, currentUser.UserId);
            return (ServiceResult.Unauthorized, null); // Or BadRequest for unknown role
        }

        var dtos = mapper.Map<List<SpecificationIdentifyingInformationHeaderDto>>(pagedEntities.Items);
        var response = new PaginatedSpecificationHeaderResponse(
            Metadata: new PaginationMetadata(
                pagedEntities.TotalCount,
                pagedEntities.PageSize,
                pagedEntities.PageNumber,
                pagedEntities.TotalPages,
                pagedEntities.HasNextPage,
                pagedEntities.HasPreviousPage
            ),
            Items: dtos
        );
        return (ServiceResult.Success, response);
    }


    public async Task<SpecificationIdentifyingInformationDetailDto?> GetSpecificationByIdAsync(int id, PaginationParams coreParams, PaginationParams extParams)
    {
        var entity = await specInfoRepo.GetByIdAsync(id);
        if (entity == null)
        {
            logger.LogWarning("Specification with ID {Id} not found.", id);
            return null;
        }

        var coreResponse = await GetSpecificationCoresAsync(id, coreParams);
        var extensionResponse = await GetSpecificationExtensionsAsync(id, extParams);

        // Ensure coreResponse and extensionResponse are not null, providing default empty responses if necessary
        // This is important because the SpecificationIdentifyingInformationDetailDto constructor expects non-null Paginated...Response objects.
        if (coreResponse == null)
        {
            logger.LogWarning("Core elements for specification ID {Id} could not be retrieved, defaulting to empty.", id);
            var defaultCoreMetadata = new PaginationMetadata(0, coreParams.PageSize, coreParams.PageNumber, 0, false, false);
            coreResponse = new PaginatedSpecificationCoreResponse(defaultCoreMetadata, new List<SpecificationCoreDto>());
        }

        if (extensionResponse == null)
        {
            logger.LogWarning("Extension elements for specification ID {Id} could not be retrieved, defaulting to empty.", id);
            var defaultExtMetadata = new PaginationMetadata(0, extParams.PageSize, extParams.PageNumber, 0, false, false);
            extensionResponse = new PaginatedSpecificationExtensionResponse(defaultExtMetadata, new List<SpecificationExtensionComponentDto>());
        }

        // Manually construct the SpecificationIdentifyingInformationDetailDto
        // This bypasses the AutoMapper issue for this specific complex DTO construction.
        var detailDto = new SpecificationIdentifyingInformationDetailDto(
            IdentityID: entity.IdentityID,
            SpecificationIdentifier: entity.SpecificationIdentifier,
            SpecificationName: entity.SpecificationName,
            Sector: entity.Sector,
            SpecificationVersion: entity.SpecificationVersion,
            DateOfImplementation: entity.DateOfImplementation,
            Country: entity.Country,
            SubSector: entity.SubSector,
            Purpose: entity.Purpose,
            ContactInformation: entity.ContactInformation,
            GoverningEntity: entity.GoverningEntity,
            CoreVersion: entity.CoreVersion,
            SpecificationSourceLink: entity.SpecificationSourceLink,
            IsCountrySpecification: entity.IsCountrySpecification,
            UnderlyingSpecificationIdentifier: entity.UnderlyingSpecificationIdentifier,
            PreferredSyntax: entity.PreferredSyntax,
            CreatedDate: entity.CreatedDate,
            ModifiedDate: entity.ModifiedDate,
            ImplementationStatus: entity.ImplementationStatus, // Map new status
            RegistrationStatus: entity.RegistrationStatus,   // Map new status
            SpecificationCores: coreResponse, // Assign the fetched PaginatedSpecificationCoreResponse
            SpecificationExtensionComponents: extensionResponse // Assign the fetched PaginatedSpecificationExtensionResponse
        );

        return detailDto;
    }

    // Modified to accept CurrentUserContext (simulated for now)
    public async Task<(ServiceResult Status, SpecificationIdentifyingInformationHeaderDto? Dto)> CreateSpecificationAsync(
        SpecificationIdentifyingInformationCreateDto createDto,
        CurrentUserContext? currentUser) // Simulate current user
    {
        if (currentUser == null)
        {
            logger.LogWarning("Attempt to create specification without user context.");
            return (ServiceResult.Unauthorized, null); // Or BadRequest
        }

        var entity = mapper.Map<SpecificationIdentifyingInformation>(createDto);

        entity.CreatedDate = DateTime.UtcNow;
        entity.ModifiedDate = DateTime.UtcNow;

        // Set default statuses if not provided in DTO, or rely on DTO defaults
        if (string.IsNullOrWhiteSpace(entity.ImplementationStatus))
        {
            entity.ImplementationStatus = "Planned"; // Default if not set by DTO/mapper
        }
        if (string.IsNullOrWhiteSpace(entity.RegistrationStatus))
        {
            entity.RegistrationStatus = "Submitted"; // Default if not set by DTO/mapper
        }


        // Assign UserGroupID based on current user, as per plan
        if (currentUser.Role == "User")
        {
            if (!currentUser.UserGroupId.HasValue)
            {
                logger.LogWarning("User {UserId} attempted to create specification but has no UserGroupID.", currentUser.UserId);
                return (ServiceResult.Forbidden, null); // User must belong to a group to create specs
            }
            entity.UserGroupID = currentUser.UserGroupId;
        }
        else if (currentUser.Role == "Admin")
        {
            entity.UserGroupID = null;
        }
        else
        {
            logger.LogError("Unknown role {Role} for user {UserId} attempting to create specification.", currentUser.Role, currentUser.UserId);
            return (ServiceResult.Unauthorized, null);
        }

        await specInfoRepo.AddAsync(entity);
        if (!await specInfoRepo.SaveChangesAsync())
        {
            logger.LogError("Failed to save new specification.");
            return (ServiceResult.BadRequest, null);
        }
        return (ServiceResult.Success, mapper.Map<SpecificationIdentifyingInformationHeaderDto>(entity));
    }

    // Modified to accept CurrentUserContext
    public async Task<ServiceResult> UpdateSpecificationAsync(
        int id,
        SpecificationIdentifyingInformationUpdateDto updateDto,
        CurrentUserContext? currentUser) // Simulate current user
    {
        var entity = await specInfoRepo.GetByIdAsync(id);
        if (entity == null) return ServiceResult.NotFound;

        if (!await CanUserEditSpecification(id, currentUser))
        {
            logger.LogWarning("User {UserId} (Role: {UserRole}, Group: {UserGroupId}) attempted to update specification {SpecificationId} without permission.",
                currentUser?.UserId, currentUser?.Role, currentUser?.UserGroupId, id);
            return ServiceResult.Forbidden;
        }

        mapper.Map(updateDto, entity);
        entity.ModifiedDate = DateTime.UtcNow;

        specInfoRepo.Update(entity);
        if (!await specInfoRepo.SaveChangesAsync())
        {
            logger.LogError("Failed to save updated specification {SpecificationId}.", id);
            return ServiceResult.BadRequest;
        }
        return ServiceResult.Success;
    }

    // Modified to accept CurrentUserContext
    public async Task<DeleteResult> DeleteSpecificationAsync(int id, CurrentUserContext? currentUser)
    {
        var entity = await specInfoRepo.GetByIdAsync(id);
        if (entity == null) return DeleteResult.NotFound;

        if (!await CanUserEditSpecification(id, currentUser))
        {
            logger.LogWarning("User {UserId} (Role: {UserRole}, Group: {UserGroupId}) attempted to delete specification {SpecificationId} without permission.",
               currentUser?.UserId, currentUser?.Role, currentUser?.UserGroupId, id);
            return DeleteResult.Forbidden;
        }

        bool hasCore = await specInfoRepo.HasCoreElementsAsync(id);
        bool hasExt = await specInfoRepo.HasExtensionComponentsAsync(id);

        if (hasCore || hasExt)
        {
            return DeleteResult.Conflict;
        }

        specInfoRepo.Delete(entity);
        if (!await specInfoRepo.SaveChangesAsync())
        {
            logger.LogError("Failed to delete specification {SpecificationId}.", id);
            return DeleteResult.Error;
        }
        return DeleteResult.Success;
    }

    // New method for Admin to assign/change group
    public async Task<ServiceResult> AssignSpecificationToGroupAsync(int specificationId, int? userGroupId, CurrentUserContext? currentUser)
    {
        if (currentUser == null || currentUser.Role != "Admin")
        {
            return ServiceResult.Forbidden;
        }

        var spec = await specInfoRepo.GetByIdAsync(specificationId);
        if (spec == null)
        {
            return ServiceResult.NotFound;
        }

        spec.UserGroupID = userGroupId;
        spec.ModifiedDate = DateTime.UtcNow;
        specInfoRepo.Update(spec);

        if (!await specInfoRepo.SaveChangesAsync())
        {
            logger.LogError("Admin {AdminId} failed to assign specification {SpecificationId} to group {UserGroupId}.", currentUser.UserId, specificationId, userGroupId);
            return ServiceResult.BadRequest;
        }
        return ServiceResult.Success;
    }


    // --- Specification Core Methods ---

    public async Task<PaginatedSpecificationCoreResponse?> GetSpecificationCoresAsync(int specificationId, PaginationParams paginationParams)
    {
        if (!await specInfoRepo.ExistsAsync(specificationId)) return null;
        var pagedEntities = await specCoreRepo.GetBySpecificationIdPaginatedAsync(specificationId, paginationParams);
        var dtos = mapper.Map<List<SpecificationCoreDto>>(pagedEntities.Items);
        return new PaginatedSpecificationCoreResponse(
            new PaginationMetadata(pagedEntities.TotalCount, pagedEntities.PageSize, pagedEntities.PageNumber, pagedEntities.TotalPages, pagedEntities.HasNextPage, pagedEntities.HasPreviousPage),
            dtos
        );
    }

    public async Task<SpecificationCoreDto?> GetSpecificationCoreByIdAsync(int specificationId, int coreElementId)
    {
        var entity = await specCoreRepo.GetByIdAndSpecificationIdAsync(coreElementId, specificationId);
        return mapper.Map<SpecificationCoreDto>(entity);
    }

    public async Task<(ServiceResult Status, SpecificationCoreDto? Dto)> AddCoreElementAsync(int specificationId, SpecificationCoreCreateDto createDto, CurrentUserContext? currentUser)
    {
        if (!await CanUserEditSpecification(specificationId, currentUser)) return (ServiceResult.Forbidden, null);
        if (!await specInfoRepo.ExistsAsync(specificationId)) return (ServiceResult.NotFound, null);
        if (!await specCoreRepo.CoreInvoiceModelExistsAsync(createDto.BusinessTermID)) return (ServiceResult.RefNotFound, null);

        var entity = mapper.Map<SpecificationCore>(createDto);
        entity.IdentityID = specificationId;
        await specCoreRepo.AddAsync(entity);

        var parentSpec = await specInfoRepo.GetByIdAsync(specificationId);
        if (parentSpec != null)
        {
            parentSpec.ModifiedDate = DateTime.UtcNow;
            specInfoRepo.Update(parentSpec);
        }

        bool saved = await specCoreRepo.SaveChangesAsync();
        return saved ? (ServiceResult.Success, mapper.Map<SpecificationCoreDto>(entity)) : (ServiceResult.BadRequest, null);
    }

    public async Task<ServiceResult> UpdateCoreElementAsync(int specificationId, int coreElementId, SpecificationCoreUpdateDto updateDto, CurrentUserContext? currentUser)
    {
        if (!await CanUserEditSpecification(specificationId, currentUser)) return ServiceResult.Forbidden;
        var entity = await specCoreRepo.GetByIdAndSpecificationIdAsync(coreElementId, specificationId);
        if (entity == null) return ServiceResult.NotFound;
        if (updateDto.BusinessTermID != entity.BusinessTermID && !await specCoreRepo.CoreInvoiceModelExistsAsync(updateDto.BusinessTermID)) return ServiceResult.RefNotFound;

        mapper.Map(updateDto, entity);
        specCoreRepo.Update(entity);

        var parentSpec = await specInfoRepo.GetByIdAsync(specificationId);
        if (parentSpec != null)
        {
            parentSpec.ModifiedDate = DateTime.UtcNow;
            specInfoRepo.Update(parentSpec);
        }
        bool saved = await specCoreRepo.SaveChangesAsync();
        return saved ? ServiceResult.Success : ServiceResult.BadRequest;
    }

    public async Task<ServiceResult> DeleteCoreElementAsync(int specificationId, int coreElementId, CurrentUserContext? currentUser)
    {
        if (!await CanUserEditSpecification(specificationId, currentUser)) return ServiceResult.Forbidden;
        var entity = await specCoreRepo.GetByIdAndSpecificationIdAsync(coreElementId, specificationId);
        if (entity == null) return ServiceResult.NotFound;

        specCoreRepo.Delete(entity);
        var parentSpec = await specInfoRepo.GetByIdAsync(specificationId);
        if (parentSpec != null)
        {
            parentSpec.ModifiedDate = DateTime.UtcNow;
            specInfoRepo.Update(parentSpec);
        }
        bool saved = await specCoreRepo.SaveChangesAsync();
        return saved ? ServiceResult.Success : ServiceResult.BadRequest;
    }

    // --- Specification Extension Methods ---

    public async Task<PaginatedSpecificationExtensionResponse?> GetSpecificationExtensionsAsync(int specificationId, PaginationParams paginationParams)
    {
        if (!await specInfoRepo.ExistsAsync(specificationId)) return null;
        var pagedEntities = await specExtRepo.GetBySpecificationIdPaginatedAsync(specificationId, paginationParams);
        var dtos = mapper.Map<List<SpecificationExtensionComponentDto>>(pagedEntities.Items);
        return new PaginatedSpecificationExtensionResponse(
            new PaginationMetadata(pagedEntities.TotalCount, pagedEntities.PageSize, pagedEntities.PageNumber, pagedEntities.TotalPages, pagedEntities.HasNextPage, pagedEntities.HasPreviousPage),
            dtos
        );
    }

    public async Task<SpecificationExtensionComponentDto?> GetSpecificationExtensionByIdAsync(int specificationId, int extensionElementId)
    {
        var entity = await specExtRepo.GetByIdAndSpecificationIdAsync(extensionElementId, specificationId);
        return mapper.Map<SpecificationExtensionComponentDto>(entity);
    }

    public async Task<(ServiceResult Status, SpecificationExtensionComponentDto? Dto)> AddExtensionElementAsync(int specificationId, SpecificationExtensionComponentCreateDto createDto, CurrentUserContext? currentUser)
    {
        if (!await CanUserEditSpecification(specificationId, currentUser)) return (ServiceResult.Forbidden, null);
        if (!await specInfoRepo.ExistsAsync(specificationId)) return (ServiceResult.NotFound, null);
        if (!await specExtRepo.ExtensionElementExistsAsync(createDto.ExtensionComponentID, createDto.BusinessTermID)) return (ServiceResult.RefNotFound, null);

        var entity = mapper.Map<SpecificationExtensionComponent>(createDto);
        entity.IdentityID = specificationId;
        await specExtRepo.AddAsync(entity);

        var parentSpec = await specInfoRepo.GetByIdAsync(specificationId);
        if (parentSpec != null)
        {
            parentSpec.ModifiedDate = DateTime.UtcNow;
            specInfoRepo.Update(parentSpec);
        }
        bool saved = await specExtRepo.SaveChangesAsync();
        return saved ? (ServiceResult.Success, mapper.Map<SpecificationExtensionComponentDto>(entity)) : (ServiceResult.BadRequest, null);
    }

    public async Task<ServiceResult> UpdateExtensionElementAsync(int specificationId, int extensionElementId, SpecificationExtensionComponentUpdateDto updateDto, CurrentUserContext? currentUser)
    {
        if (!await CanUserEditSpecification(specificationId, currentUser)) return ServiceResult.Forbidden;
        var entity = await specExtRepo.GetByIdAndSpecificationIdAsync(extensionElementId, specificationId);
        if (entity == null) return ServiceResult.NotFound;
        if ((updateDto.ExtensionComponentID != entity.ExtensionComponentID || updateDto.BusinessTermID != entity.BusinessTermID) &&
            !await specExtRepo.ExtensionElementExistsAsync(updateDto.ExtensionComponentID, updateDto.BusinessTermID))
        {
            return ServiceResult.RefNotFound;
        }

        mapper.Map(updateDto, entity);
        specExtRepo.Update(entity);
        var parentSpec = await specInfoRepo.GetByIdAsync(specificationId);
        if (parentSpec != null)
        {
            parentSpec.ModifiedDate = DateTime.UtcNow;
            specInfoRepo.Update(parentSpec);
        }
        bool saved = await specExtRepo.SaveChangesAsync();
        return saved ? ServiceResult.Success : ServiceResult.BadRequest;
    }

    public async Task<ServiceResult> DeleteExtensionElementAsync(int specificationId, int extensionElementId, CurrentUserContext? currentUser)
    {
        if (!await CanUserEditSpecification(specificationId, currentUser)) return ServiceResult.Forbidden;
        var entity = await specExtRepo.GetByIdAndSpecificationIdAsync(extensionElementId, specificationId);
        if (entity == null) return ServiceResult.NotFound;

        specExtRepo.Delete(entity);
        var parentSpec = await specInfoRepo.GetByIdAsync(specificationId);
        if (parentSpec != null)
        {
            parentSpec.ModifiedDate = DateTime.UtcNow;
            specInfoRepo.Update(parentSpec);
        }
        bool saved = await specExtRepo.SaveChangesAsync();
        return saved ? ServiceResult.Success : ServiceResult.BadRequest;
    }
}
