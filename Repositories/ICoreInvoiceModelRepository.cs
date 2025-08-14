// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using RegistryApi.Models;
using RegistryApi.DTOs;
using RegistryApi.Helpers;

namespace RegistryApi.Repositories;

/// <summary>
/// Interface for the CoreInvoiceModel repository.
/// </summary>
public interface ICoreInvoiceModelRepository : IGenericRepository<CoreInvoiceModel>
{
    /// <summary>
    /// Retrieves a paginated list of all CoreInvoiceModel items, ordered by RowPos.
    /// </summary>
    /// <param name="paginationParams">Parameters for pagination.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paged list of CoreInvoiceModelDto.</returns>
    Task<PagedList<CoreInvoiceModelDto>> GetAllAsync(PaginationParams paginationParams);
}
