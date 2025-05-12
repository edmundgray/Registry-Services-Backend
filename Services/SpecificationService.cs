using AutoMapper;
using RegistryApi.DTOs;
using RegistryApi.Models;
using RegistryApi.Repositories;
using RegistryApi.Helpers; // For PagedList

namespace RegistryApi.Services;

// Using primary constructor for dependency injection
public class SpecificationService(
    ISpecificationIdentifyingInformationRepository specInfoRepo,
    ISpecificationCoreRepository specCoreRepo,
    ISpecificationExtensionComponentRepository specExtRepo,
    IMapper mapper // Inject AutoMapper
    ) : ISpecificationService
{
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

    public async Task<SpecificationIdentifyingInformationDetailDto?> GetSpecificationByIdAsync(int id, PaginationParams coreParams, PaginationParams extParams)
    {
        var entity = await specInfoRepo.GetByIdAsync(id); // Just get the header first
        if (entity == null) return null;

        // Map header info
        // Note: AutoMapper needs configuration for mapping record types if not straightforward
        var headerDto = mapper.Map<SpecificationIdentifyingInformationHeaderDto>(entity);

        // Get paginated children separately
        var coreResponse = await GetSpecificationCoresAsync(id, coreParams);
        var extensionResponse = await GetSpecificationExtensionsAsync(id, extParams);

        // Check if child fetches were successful (spec ID was valid)
        if (coreResponse == null || extensionResponse == null)
        {
             // Should not happen if entity was found, but defensive check
             return null;
        }

        // Combine into the detail DTO
        return new SpecificationIdentifyingInformationDetailDto(
            headerDto.IdentityID,
            headerDto.SpecificationIdentifier,
            headerDto.SpecificationName,
            headerDto.Sector,
            headerDto.SpecificationVersion,
            headerDto.DateOfImplementation,
            headerDto.Country,
            entity.SubSector, // Map remaining fields from the entity
            entity.Purpose,
            entity.ContactInformation,
            entity.GoverningEntity,
            entity.CoreVersion,
            entity.SpecificationSourceLink,
            entity.IsCountrySpecification,
            entity.UnderlyingSpecificationIdentifier,
            entity.PreferredSyntax,
            coreResponse,
            extensionResponse
        );
    }

    public async Task<SpecificationIdentifyingInformationHeaderDto?> CreateSpecificationAsync(SpecificationIdentifyingInformationCreateDto createDto)
    {
        var entity = mapper.Map<SpecificationIdentifyingInformation>(createDto);
        await specInfoRepo.AddAsync(entity);
        await specInfoRepo.SaveChangesAsync();
        return mapper.Map<SpecificationIdentifyingInformationHeaderDto>(entity);
    }

    public async Task<bool> UpdateSpecificationAsync(int id, SpecificationIdentifyingInformationUpdateDto updateDto)
    {
        var entity = await specInfoRepo.GetByIdAsync(id);
        if (entity == null) return false;

        mapper.Map(updateDto, entity); // Map updates onto the existing entity
        specInfoRepo.Update(entity);
        return await specInfoRepo.SaveChangesAsync();
    }

    public async Task<DeleteResult> DeleteSpecificationAsync(int id)
    {
        // Check existence first
        var entity = await specInfoRepo.GetByIdAsync(id);
         if (entity == null) return DeleteResult.NotFound;

        // Check for children
        bool hasCore = await specInfoRepo.HasCoreElementsAsync(id);
        bool hasExt = await specInfoRepo.HasExtensionComponentsAsync(id);

        if (hasCore || hasExt)
        {
            return DeleteResult.Conflict; // Indicate conflict (children exist)
        }

        specInfoRepo.Delete(entity);
        bool deleted = await specInfoRepo.SaveChangesAsync();
        return deleted ? DeleteResult.Success : DeleteResult.NotFound; // Or some other error
    }


    // --- Specification Core Methods ---

    public async Task<PaginatedSpecificationCoreResponse?> GetSpecificationCoresAsync(int specificationId, PaginationParams paginationParams)
    {
         // Check if parent exists first for better error handling upstream
         if (!await specInfoRepo.ExistsAsync(specificationId)) return null;

         var pagedEntities = await specCoreRepo.GetBySpecificationIdPaginatedAsync(specificationId, paginationParams);
         var dtos = mapper.Map<List<SpecificationCoreDto>>(pagedEntities.Items);
         return new PaginatedSpecificationCoreResponse
         (
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

    public async Task<SpecificationCoreDto?> GetSpecificationCoreByIdAsync(int specificationId, int coreElementId)
    {
         var entity = await specCoreRepo.GetByIdAndSpecificationIdAsync(coreElementId, specificationId);
         return mapper.Map<SpecificationCoreDto>(entity);
    }


    public async Task<(ServiceResult Status, SpecificationCoreDto? Dto)> AddCoreElementAsync(int specificationId, SpecificationCoreCreateDto createDto)
    {
        if (!await specInfoRepo.ExistsAsync(specificationId))
            return (ServiceResult.NotFound, null); // Parent Spec not found

        if (!await specCoreRepo.CoreInvoiceModelExistsAsync(createDto.BusinessTermID))
            return (ServiceResult.RefNotFound, null); // Referenced Core Model element not found

        var entity = mapper.Map<SpecificationCore>(createDto);
        entity.IdentityID = specificationId; // Set the foreign key

        await specCoreRepo.AddAsync(entity);
        bool saved = await specCoreRepo.SaveChangesAsync();

        return saved
            ? (ServiceResult.Success, mapper.Map<SpecificationCoreDto>(entity))
            : (ServiceResult.BadRequest, null); // Indicate save failure
    }

    public async Task<ServiceResult> UpdateCoreElementAsync(int specificationId, int coreElementId, SpecificationCoreUpdateDto updateDto)
    {
        var entity = await specCoreRepo.GetByIdAndSpecificationIdAsync(coreElementId, specificationId);
        if (entity == null) return ServiceResult.NotFound;

        // Optional: Validate new BusinessTermID if it's changeable
        if (updateDto.BusinessTermID != entity.BusinessTermID && !await specCoreRepo.CoreInvoiceModelExistsAsync(updateDto.BusinessTermID))
            return ServiceResult.RefNotFound;

        mapper.Map(updateDto, entity);
        specCoreRepo.Update(entity);
        bool saved = await specCoreRepo.SaveChangesAsync();
        return saved ? ServiceResult.Success : ServiceResult.BadRequest; // Indicate save failure
    }

    public async Task<ServiceResult> DeleteCoreElementAsync(int specificationId, int coreElementId)
    {
        var entity = await specCoreRepo.GetByIdAndSpecificationIdAsync(coreElementId, specificationId);
        if (entity == null) return ServiceResult.NotFound;

        specCoreRepo.Delete(entity);
        bool saved = await specCoreRepo.SaveChangesAsync();
         return saved ? ServiceResult.Success : ServiceResult.BadRequest; // Indicate save failure
    }

    // --- Specification Extension Methods ---

    public async Task<PaginatedSpecificationExtensionResponse?> GetSpecificationExtensionsAsync(int specificationId, PaginationParams paginationParams)
    {
         if (!await specInfoRepo.ExistsAsync(specificationId)) return null;

        var pagedEntities = await specExtRepo.GetBySpecificationIdPaginatedAsync(specificationId, paginationParams);
        var dtos = mapper.Map<List<SpecificationExtensionComponentDto>>(pagedEntities.Items);
        return new PaginatedSpecificationExtensionResponse
        (
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

     public async Task<SpecificationExtensionComponentDto?> GetSpecificationExtensionByIdAsync(int specificationId, int extensionElementId)
    {
         var entity = await specExtRepo.GetByIdAndSpecificationIdAsync(extensionElementId, specificationId);
         return mapper.Map<SpecificationExtensionComponentDto>(entity);
    }


    public async Task<(ServiceResult Status, SpecificationExtensionComponentDto? Dto)> AddExtensionElementAsync(int specificationId, SpecificationExtensionComponentCreateDto createDto)
    {
        if (!await specInfoRepo.ExistsAsync(specificationId))
            return (ServiceResult.NotFound, null);

        if (!await specExtRepo.ExtensionElementExistsAsync(createDto.ExtensionComponentID, createDto.BusinessTermID))
            return (ServiceResult.RefNotFound, null); // Ref Element not found

        var entity = mapper.Map<SpecificationExtensionComponent>(createDto);
        entity.IdentityID = specificationId;

        await specExtRepo.AddAsync(entity);
        bool saved = await specExtRepo.SaveChangesAsync();

        return saved
            ? (ServiceResult.Success, mapper.Map<SpecificationExtensionComponentDto>(entity))
            : (ServiceResult.BadRequest, null);
    }

    public async Task<ServiceResult> UpdateExtensionElementAsync(int specificationId, int extensionElementId, SpecificationExtensionComponentUpdateDto updateDto)
    {
        var entity = await specExtRepo.GetByIdAndSpecificationIdAsync(extensionElementId, specificationId);
        if (entity == null) return ServiceResult.NotFound;

        // Optional: Validate new composite key if changeable
        if ((updateDto.ExtensionComponentID != entity.ExtensionComponentID || updateDto.BusinessTermID != entity.BusinessTermID) &&
            !await specExtRepo.ExtensionElementExistsAsync(updateDto.ExtensionComponentID, updateDto.BusinessTermID))
        {
            return ServiceResult.RefNotFound;
        }

        mapper.Map(updateDto, entity);
        specExtRepo.Update(entity);
        bool saved = await specExtRepo.SaveChangesAsync();
        return saved ? ServiceResult.Success : ServiceResult.BadRequest;
    }

    public async Task<ServiceResult> DeleteExtensionElementAsync(int specificationId, int extensionElementId)
    {
        var entity = await specExtRepo.GetByIdAndSpecificationIdAsync(extensionElementId, specificationId);
        if (entity == null) return ServiceResult.NotFound;

        specExtRepo.Delete(entity);
        bool saved = await specExtRepo.SaveChangesAsync();
        return saved ? ServiceResult.Success : ServiceResult.BadRequest;
    }
}
