// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using RegistryApi.Models;
using RegistryApi.DTOs; // For PaginationParams
using RegistryApi.Helpers; // For PagedList

namespace RegistryApi.Repositories;


public interface ISpecificationIdentifyingInformationRepository : IGenericRepository<SpecificationIdentifyingInformation>
{

    Task<PagedList<SpecificationIdentifyingInformation>> GetAllPaginatedAsync(PaginationParams paginationParams, bool includeSubmittedAndInProgress = false);
    Task<IEnumerable<SpecificationIdentifyingInformation>> GetByUserGroupIdAsync(int userGroupId);
    Task<IEnumerable<SpecificationIdentifyingInformation>> GetAllAsync(bool includeSubmittedAndInProgress = false);

    Task<bool> HasCoreElementsAsync(int id);
    Task<bool> HasExtensionComponentsAsync(int id);
    Task<bool> ExistsAsync(int id);
}
