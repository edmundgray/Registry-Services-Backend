using AutoMapper;
using RegistryApi.DTOs;
using RegistryApi.Models;
using RegistryApi.Repositories;
using RegistryApi.Helpers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace RegistryApi.Services;

public record CurrentUserContext(int UserId, string Role, int? UserGroupId);

public class SpecificationService(
    ISpecificationIdentifyingInformationRepository specInfoRepo,
    ISpecificationCoreRepository specCoreRepo,
    ISpecificationExtensionComponentRepository specExtRepo,
    IUserGroupRepository userGroupRepo,
    IMapper mapper,
    ILogger<SpecificationService> logger
    ) : ISpecificationService
{
    private async Task<bool> CanUserEditSpecification(int specificationId, CurrentUserContext? currentUser)
    {
        if (currentUser == null)
        {
            logger.LogWarning("Permission check (CanUserEditSpecification) failed: CurrentUserContext is null for specification ID {SpecificationId}.", specificationId);
            return false;
        }
        if (currentUser.Role == "Admin")
        {
            return true;
        }

        var spec = await specInfoRepo.GetByIdAsync(specificationId);
        if (spec == null)
        {
            logger.LogWarning("Permission check (CanUserEditSpecification) failed: Specification with ID {SpecificationId} not found.", specificationId);
            return false;
        }

        bool canEdit = spec.UserGroupID.HasValue && spec.UserGroupID == currentUser.UserGroupId;
        if (!canEdit)
        {
            logger.LogWarning("User {UserId} (Role: {UserRole}, Group: {UserGroupId}) does not have permission to edit specification {SpecificationId} (Owned by Group: {OwnerGroupId}).",
                currentUser.UserId, currentUser.Role, currentUser.UserGroupId, specificationId, spec.UserGroupID);
        }
        return canEdit;
    }

    public async Task<PaginatedSpecificationHeaderResponse> GetSpecificationsAsync(PaginationParams paginationParams)
    {
        var pagedEntities = await specInfoRepo.GetAllPaginatedAsync(paginationParams, includeSubmitted: false);

        var dtos = pagedEntities.Items.Select(entity => new SpecificationIdentifyingInformationHeaderDto(
            IdentityID: entity.IdentityID,
            SpecificationIdentifier: entity.SpecificationIdentifier,
            SpecificationName: entity.SpecificationName,
            Sector: entity.Sector,
            SpecificationVersion: entity.SpecificationVersion,
            DateOfImplementation: entity.DateOfImplementation,
            Country: entity.Country,
            CreatedDate: entity.CreatedDate,
            ModifiedDate: entity.ModifiedDate,
            ImplementationStatus: entity.ImplementationStatus,
            RegistrationStatus: entity.RegistrationStatus,
            SpecificationType: entity.SpecificationType,
            ConformanceLevel: entity.ConformanceLevel,
            Purpose: entity.Purpose,
            PreferredSyntax: entity.PreferredSyntax,
            GoverningEntity: entity.UserGroup?.GroupName,
            UserGroupID: entity.UserGroupID
        )).ToList();

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

    public async Task<PaginatedSpecificationHeaderResponse> GetAdminSpecificationsAsync(PaginationParams paginationParams)
    {
        var pagedEntities = await specInfoRepo.GetAllPaginatedAsync(paginationParams, includeSubmitted: true);

        var dtos = pagedEntities.Items.Select(entity => new SpecificationIdentifyingInformationHeaderDto(
            IdentityID: entity.IdentityID,
            SpecificationIdentifier: entity.SpecificationIdentifier,
            SpecificationName: entity.SpecificationName,
            Sector: entity.Sector,
            SpecificationVersion: entity.SpecificationVersion,
            DateOfImplementation: entity.DateOfImplementation,
            Country: entity.Country,
            CreatedDate: entity.CreatedDate,
            ModifiedDate: entity.ModifiedDate,
            ImplementationStatus: entity.ImplementationStatus,
            RegistrationStatus: entity.RegistrationStatus,
            SpecificationType: entity.SpecificationType,
            ConformanceLevel: entity.ConformanceLevel,
            Purpose: entity.Purpose,
            PreferredSyntax: entity.PreferredSyntax,
            GoverningEntity: entity.UserGroup?.GroupName,
            UserGroupID: entity.UserGroupID
        )).ToList();

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
        CurrentUserContext? currentUser,
        PaginationParams paginationParams)
    {
        if (currentUser == null)
        {
            logger.LogWarning("GetSpecificationsByUserGroupAsync called with null CurrentUserContext.");
            return (ServiceResult.Unauthorized, null);
        }

        PagedList<SpecificationIdentifyingInformation> pagedEntities;

        if (currentUser.Role == "Admin")
        {
            pagedEntities = await specInfoRepo.GetAllPaginatedAsync(paginationParams);
        }
        else if (currentUser.Role == "User")
        {
            if (!currentUser.UserGroupId.HasValue)
            {
                logger.LogWarning("User {UserId} (Role: User) attempted to get group specifications but has no UserGroupID.", currentUser.UserId);
                return (ServiceResult.Forbidden, null);
            }
            pagedEntities = await specInfoRepo.GetByUserGroupIdPaginatedAsync(currentUser.UserGroupId.Value, paginationParams);
        }
        else
        {
            logger.LogError("Unknown role {Role} for user {UserId} attempting to get group specifications.", currentUser.Role, currentUser.UserId);
            return (ServiceResult.Unauthorized, null);
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
        if (coreResponse == null)
        {
            var defaultCoreMetadata = new PaginationMetadata(0, coreParams.PageSize, coreParams.PageNumber, 0, false, false);
            coreResponse = new PaginatedSpecificationCoreResponse(defaultCoreMetadata, new List<SpecificationCoreDto>());
        }

        var extensionResponse = await GetSpecificationExtensionsAsync(id, extParams);
        if (extensionResponse == null)
        {
            var defaultExtMetadata = new PaginationMetadata(0, extParams.PageSize, extParams.PageNumber, 0, false, false);
            extensionResponse = new PaginatedSpecificationExtensionResponse(defaultExtMetadata, new List<SpecificationExtensionComponentDto>());
        }

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
            GoverningEntity: entity.UserGroup?.GroupName,
            UserGroupID: entity.UserGroupID,
            CoreVersion: entity.CoreVersion,
            SpecificationSourceLink: entity.SpecificationSourceLink,
            IsCountrySpecification: entity.IsCountrySpecification,
            UnderlyingSpecificationIdentifier: entity.UnderlyingSpecificationIdentifier,
            PreferredSyntax: entity.PreferredSyntax,
            CreatedDate: entity.CreatedDate,
            ModifiedDate: entity.ModifiedDate,
            ImplementationStatus: entity.ImplementationStatus,
            RegistrationStatus: entity.RegistrationStatus,
            SpecificationType: entity.SpecificationType,
            ConformanceLevel: entity.ConformanceLevel,
            SpecificationCores: coreResponse,
            SpecificationExtensionComponents: extensionResponse
        );

        return detailDto;
    }

    public async Task<(ServiceResult Status, SpecificationIdentifyingInformationHeaderDto? Dto)> CreateSpecificationAsync(
        SpecificationIdentifyingInformationCreateDto createDto,
        CurrentUserContext? currentUser)
    {
        if (currentUser == null)
        {
            logger.LogWarning("Attempt to create specification without user context or with invalid claims.");
            return (ServiceResult.Unauthorized, null);
        }

        if (createDto.UserGroupID.HasValue)
        {
            var groupExists = await userGroupRepo.GetByIdAsync(createDto.UserGroupID.Value);
            if (groupExists == null)
            {
                logger.LogWarning("Attempt to create specification with non-existent UserGroupID {UserGroupId}", createDto.UserGroupID.Value);
                return (ServiceResult.RefNotFound, null);
            }
        }

        var entity = mapper.Map<SpecificationIdentifyingInformation>(createDto);
        entity.CreatedDate = DateTime.UtcNow;
        entity.ModifiedDate = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(entity.ImplementationStatus)) entity.ImplementationStatus = "Planned";
        if (string.IsNullOrWhiteSpace(entity.RegistrationStatus)) entity.RegistrationStatus = "Submitted";

        if (currentUser.Role == "Admin")
        {
            entity.UserGroupID = createDto.UserGroupID;
        }
        else
        {
            if (!currentUser.UserGroupId.HasValue)
            {
                logger.LogWarning("User {UserId} (Role: User) attempted to create specification but has no UserGroupID.", currentUser.UserId);
                return (ServiceResult.Forbidden, null);
            }
            entity.UserGroupID = currentUser.UserGroupId.Value;
        }

        await specInfoRepo.AddAsync(entity);
        if (!await specInfoRepo.SaveChangesAsync())
        {
            logger.LogError("Failed to save new specification for user {UserId}.", currentUser.UserId);
            return (ServiceResult.BadRequest, null);
        }

        var createdEntity = await specInfoRepo.GetByIdAsync(entity.IdentityID);
        return (ServiceResult.Success, mapper.Map<SpecificationIdentifyingInformationHeaderDto>(createdEntity));
    }

    public async Task<ServiceResult> UpdateSpecificationAsync(
        int id,
        SpecificationIdentifyingInformationUpdateDto updateDto,
        CurrentUserContext? currentUser)
    {
        if (currentUser == null)
        {
            logger.LogWarning("Attempt to update specification {SpecificationId} without user context or with invalid claims.", id);
            return ServiceResult.Unauthorized;
        }

        var entity = await specInfoRepo.GetByIdAsync(id);
        if (entity == null) return ServiceResult.NotFound;

        if (!await CanUserEditSpecification(id, currentUser))
        {
            return ServiceResult.Forbidden;
        }

        if (currentUser.Role != "Admin" && updateDto.UserGroupID != entity.UserGroupID)
        {
            logger.LogWarning("User {UserId} attempted to change UserGroupID on specification {SpecificationId} but is not an Admin.", currentUser.UserId, id);
            return ServiceResult.Forbidden;
        }

        if (updateDto.UserGroupID.HasValue)
        {
            var groupExists = await userGroupRepo.GetByIdAsync(updateDto.UserGroupID.Value);
            if (groupExists == null)
            {
                logger.LogWarning("Attempt to update specification {SpecificationId} with non-existent UserGroupID {UserGroupId}", id, updateDto.UserGroupID.Value);
                return ServiceResult.RefNotFound;
            }
        }

        mapper.Map(updateDto, entity);
        entity.ModifiedDate = DateTime.UtcNow;

        specInfoRepo.Update(entity);

        try
        {
            if (!await specInfoRepo.SaveChangesAsync())
            {
                logger.LogError("Failed to save updated specification {SpecificationId} by user {UserId}. SaveChangesAsync returned false.", id, currentUser.UserId);
                return ServiceResult.BadRequest;
            }
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update exception for specification {SpecificationId}. A foreign key constraint likely failed.", id);
            return ServiceResult.Conflict;
        }

        return ServiceResult.Success;
    }

    public async Task<DeleteResult> DeleteSpecificationAsync(int id, CurrentUserContext? currentUser)
    {
        if (currentUser == null)
        {
            logger.LogWarning("Attempt to delete specification {SpecificationId} without user context or with invalid claims.", id);
            return DeleteResult.Forbidden;
        }

        var entity = await specInfoRepo.GetByIdAsync(id);
        if (entity == null) return DeleteResult.NotFound;

        if (!await CanUserEditSpecification(id, currentUser))
        {
            return DeleteResult.Forbidden;
        }

        bool hasCore = await specInfoRepo.HasCoreElementsAsync(id);
        bool hasExt = await specInfoRepo.HasExtensionComponentsAsync(id);

        if (hasCore || hasExt)
        {
            logger.LogInformation("Attempt to delete specification {SpecificationId} by user {UserId} failed due to existing child elements.", id, currentUser.UserId);
            return DeleteResult.Conflict;
        }

        specInfoRepo.Delete(entity);
        if (!await specInfoRepo.SaveChangesAsync())
        {
            logger.LogError("Failed to delete specification {SpecificationId} by user {UserId}.", id, currentUser.UserId);
            return DeleteResult.Error;
        }
        return DeleteResult.Success;
    }

    public async Task<ServiceResult> AssignSpecificationToGroupAsync(int specificationId, int? userGroupId, CurrentUserContext? currentUser)
    {
        if (currentUser == null)
        {
            logger.LogWarning("Attempt to assign specification {SpecificationId} to group without user context.", specificationId);
            return ServiceResult.Unauthorized;
        }

        if (currentUser.Role != "Admin")
        {
            logger.LogWarning("User {UserId} (Role: {UserRole}) attempted to assign specification {SpecificationId} to group - Admin only.",
                currentUser.UserId, currentUser.Role, specificationId);
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
        if (currentUser == null) return (ServiceResult.Unauthorized, null);
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
        if (currentUser == null) return ServiceResult.Unauthorized;
        if (!await CanUserEditSpecification(specificationId, currentUser)) return ServiceResult.Forbidden;

        var entity = await specCoreRepo.GetByIdAndSpecificationIdAsync(coreElementId, specificationId);
        if (entity == null) return ServiceResult.NotFound;

        if (updateDto.BusinessTermID != entity.BusinessTermID && !await specCoreRepo.CoreInvoiceModelExistsAsync(updateDto.BusinessTermID))
        {
            return ServiceResult.RefNotFound;
        }

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
        if (currentUser == null) return ServiceResult.Unauthorized;
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
        if (currentUser == null) return (ServiceResult.Unauthorized, null);
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
        if (currentUser == null) return ServiceResult.Unauthorized;
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
        if (currentUser == null) return ServiceResult.Unauthorized;
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