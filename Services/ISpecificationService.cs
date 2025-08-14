// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

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
    Task<PaginatedSpecificationHeaderResponse> GetAdminSpecificationsAsync(PaginationParams paginationParams); // New Admin method
    Task<SpecificationIdentifyingInformationDetailDto?> GetSpecificationByIdAsync(int id);
    Task<(ServiceResult Status, SpecificationIdentifyingInformationHeaderDto? Dto)> CreateSpecificationAsync(SpecificationIdentifyingInformationCreateDto createDto, CurrentUserContext? currentUser);
    Task<ServiceResult> UpdateSpecificationAsync(int id, SpecificationIdentifyingInformationUpdateDto updateDto, CurrentUserContext? currentUser);
    Task<DeleteResult> DeleteSpecificationAsync(int id, CurrentUserContext? currentUser);
    Task<ServiceResult> AssignSpecificationToGroupAsync(int specificationId, int userGroupId, CurrentUserContext? currentUser); // New Admin method
    Task<(ServiceResult Status, IEnumerable<SpecificationIdentifyingInformationHeaderDto>? Response)> GetSpecificationsByUserGroupAsync(CurrentUserContext currentUser);

    // Specification Core Methods
    Task<IEnumerable<SpecificationCoreDto>?> GetSpecificationCoresAsync(int specificationId);
    Task<SpecificationCoreDto?> GetSpecificationCoreByIdAsync(int specificationId, int coreElementId);
    Task<(ServiceResult Status, SpecificationCoreDto? Dto)> AddCoreElementAsync(int specificationId, SpecificationCoreCreateDto createDto, CurrentUserContext? currentUser);
    Task<ServiceResult> UpdateCoreElementAsync(int specificationId, int coreElementId, SpecificationCoreUpdateDto updateDto, CurrentUserContext? currentUser);
    Task<ServiceResult> DeleteCoreElementAsync(int specificationId, int coreElementId, CurrentUserContext? currentUser);

    // Specification Extension Methods
    Task<IEnumerable<SpecificationExtensionComponentDto>?> GetSpecificationExtensionsAsync(int specificationId);
    Task<SpecificationExtensionComponentDto?> GetSpecificationExtensionByIdAsync(int specificationId, int extensionElementId);
    Task<(ServiceResult Status, SpecificationExtensionComponentDto? Dto)> AddExtensionElementAsync(int specificationId, SpecificationExtensionComponentCreateDto createDto, CurrentUserContext? currentUser);
    Task<ServiceResult> UpdateExtensionElementAsync(int specificationId, int extensionElementId, SpecificationExtensionComponentUpdateDto updateDto, CurrentUserContext? currentUser);
    Task<ServiceResult> DeleteExtensionElementAsync(int specificationId, int extensionElementId, CurrentUserContext? currentUser);

    // --- Additional Requirement Methods ---
    Task<IEnumerable<AdditionalRequirementDto>?> GetAdditionalRequirementsAsync(int specificationId);
    Task<(ServiceResult Status, AdditionalRequirementDto? Dto)> AddAdditionalRequirementAsync(int specificationId, AdditionalRequirementCreateDto createDto, CurrentUserContext? currentUser);
    Task<ServiceResult> UpdateAdditionalRequirementAsync(int specificationId, string businessTermId, AdditionalRequirementUpdateDto updateDto, CurrentUserContext? currentUser);
    Task<ServiceResult> DeleteAdditionalRequirementAsync(int specificationId, string businessTermId, CurrentUserContext? currentUser);
}