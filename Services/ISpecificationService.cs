using RegistryApi.DTOs;
using RegistryApi.Helpers;
using RegistryApi.Services; // For CurrentUserContext
using System.Threading.Tasks;

namespace RegistryApi.Services; // Ensure this matches the SpecificationService namespace

// Enums for more descriptive results from service methods
public enum DeleteResult { Success, NotFound, Conflict, Forbidden, Error } // Added Forbidden, Error
public enum ServiceResult { Success, NotFound, RefNotFound, Conflict, BadRequest, Unauthorized, Forbidden } // Added Unauthorized, Forbidden


public interface ISpecificationService
{
    // Specification Header Methods
    Task<PaginatedSpecificationHeaderResponse> GetSpecificationsAsync(PaginationParams paginationParams);
    Task<SpecificationIdentifyingInformationDetailDto?> GetSpecificationByIdAsync(int id, PaginationParams coreParams, PaginationParams extParams);
    Task<(ServiceResult Status, SpecificationIdentifyingInformationHeaderDto? Dto)> CreateSpecificationAsync(SpecificationIdentifyingInformationCreateDto createDto, CurrentUserContext? currentUser);
    Task<ServiceResult> UpdateSpecificationAsync(int id, SpecificationIdentifyingInformationUpdateDto updateDto, CurrentUserContext? currentUser);
    Task<DeleteResult> DeleteSpecificationAsync(int id, CurrentUserContext? currentUser);
    Task<ServiceResult> AssignSpecificationToGroupAsync(int specificationId, int? userGroupId, CurrentUserContext? currentUser); // New Admin method


    // Specification Core Methods
    Task<PaginatedSpecificationCoreResponse?> GetSpecificationCoresAsync(int specificationId, PaginationParams paginationParams);
    Task<SpecificationCoreDto?> GetSpecificationCoreByIdAsync(int specificationId, int coreElementId);
    Task<(ServiceResult Status, SpecificationCoreDto? Dto)> AddCoreElementAsync(int specificationId, SpecificationCoreCreateDto createDto, CurrentUserContext? currentUser);
    Task<ServiceResult> UpdateCoreElementAsync(int specificationId, int coreElementId, SpecificationCoreUpdateDto updateDto, CurrentUserContext? currentUser);
    Task<ServiceResult> DeleteCoreElementAsync(int specificationId, int coreElementId, CurrentUserContext? currentUser);

    // Specification Extension Methods
    Task<PaginatedSpecificationExtensionResponse?> GetSpecificationExtensionsAsync(int specificationId, PaginationParams paginationParams);
    Task<SpecificationExtensionComponentDto?> GetSpecificationExtensionByIdAsync(int specificationId, int extensionElementId);
    Task<(ServiceResult Status, SpecificationExtensionComponentDto? Dto)> AddExtensionElementAsync(int specificationId, SpecificationExtensionComponentCreateDto createDto, CurrentUserContext? currentUser);
    Task<ServiceResult> UpdateExtensionElementAsync(int specificationId, int extensionElementId, SpecificationExtensionComponentUpdateDto updateDto, CurrentUserContext? currentUser);
    Task<ServiceResult> DeleteExtensionElementAsync(int specificationId, int extensionElementId, CurrentUserContext? currentUser);
}
