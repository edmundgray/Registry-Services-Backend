// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using RegistryApi.Models;
using RegistryApi.DTOs;
using RegistryApi.Helpers;

namespace RegistryApi.Repositories;

public interface ISpecificationCoreRepository : IGenericRepository<SpecificationCore>
{
     Task<PagedList<SpecificationCore>> GetBySpecificationIdPaginatedAsync(int specificationId, PaginationParams paginationParams);
     Task<SpecificationCore?> GetByIdAndSpecificationIdAsync(int coreElementId, int specificationId);
     Task<bool> CoreInvoiceModelExistsAsync(string businessTermId);

    Task<IEnumerable<SpecificationCore>> GetAllBySpecificationIdAsync(int specificationId);
}
