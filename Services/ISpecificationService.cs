using RegistryApi.DTOs;
using RegistryApi.Helpers;

namespace RegistryApi.Services;

// Interface defines the contract for business logic operations
public interface ISpecificationService
{
    // Specification Header Methods
    Task<PaginatedSpecificationHeaderResponse> GetSpecificationsAsync(PaginationParams paginationParams);
    Task<SpecificationIdentifyingInformationDetailDto?> GetSpecificationByIdAsync(int id, PaginationParams coreParams, PaginationParams extParams);
    Task<SpecificationIdentifyingInformationHeaderDto?> CreateSpecificationAsync(SpecificationIdentifyingInformationCreateDto createDto);
    Task<bool> UpdateSpecificationAsync(int id, SpecificationIdentifyingInformationUpdateDto updateDto);
    // Returns enum/result object for better status indication (NotFound, Conflict, Success)
    Task<DeleteResult> DeleteSpecificationAsync(int id);

    // Specification Core Methods
    Task<PaginatedSpecificationCoreResponse?> GetSpecificationCoresAsync(int specificationId, PaginationParams paginationParams); // Null if spec not found
    Task<SpecificationCoreDto?> GetSpecificationCoreByIdAsync(int specificationId, int coreElementId);
    // Returns enum/result object for better status indication (SpecNotFound, RefNotFound, Success)
    Task<(ServiceResult Status, SpecificationCoreDto? Dto)> AddCoreElementAsync(int specificationId, SpecificationCoreCreateDto createDto);
    Task<ServiceResult> UpdateCoreElementAsync(int specificationId, int coreElementId, SpecificationCoreUpdateDto updateDto);
    Task<ServiceResult> DeleteCoreElementAsync(int specificationId, int coreElementId);

    // Specification Extension Methods
    Task<PaginatedSpecificationExtensionResponse?> GetSpecificationExtensionsAsync(int specificationId, PaginationParams paginationParams); // Null if spec not found
    Task<SpecificationExtensionComponentDto?> GetSpecificationExtensionByIdAsync(int specificationId, int extensionElementId);
    Task<(ServiceResult Status, SpecificationExtensionComponentDto? Dto)> AddExtensionElementAsync(int specificationId, SpecificationExtensionComponentCreateDto createDto);
    Task<ServiceResult> UpdateExtensionElementAsync(int specificationId, int extensionElementId, SpecificationExtensionComponentUpdateDto updateDto);
    Task<ServiceResult> DeleteExtensionElementAsync(int specificationId, int extensionElementId);
}

// Enums for more descriptive results from service methods
public enum DeleteResult { Success, NotFound, Conflict }
public enum ServiceResult { Success, NotFound, RefNotFound, Conflict, BadRequest } // Added BadRequest
