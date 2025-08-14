// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using RegistryApi.Models;
using RegistryApi.DTOs;
using RegistryApi.Helpers;
using System.Threading.Tasks; // Added for Task

namespace RegistryApi.Repositories;

/// <summary>
/// Interface for the ExtensionComponentsModelHeader repository.
/// </summary>
public interface IExtensionComponentsModelHeaderRepository : IGenericRepository<ExtensionComponentsModelHeader>
{
    /// <summary>
    /// Retrieves a paginated list of all ExtensionComponentsModelHeader items.
    /// </summary>
    /// <param name="paginationParams">Parameters for pagination.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paged list of ExtensionComponentsModelHeaderDto.</returns>
    Task<PagedList<ExtensionComponentsModelHeaderDto>> GetAllAsync(PaginationParams paginationParams);

    /// <summary>
    /// Retrieves an ExtensionComponentsModelHeader by its string ID.
    /// </summary>
    /// <param name="id">The string ID of the header.</param>
    /// <returns>The ExtensionComponentsModelHeader entity if found, otherwise null.</returns>
    Task<ExtensionComponentsModelHeader?> GetByIdAsync(string id); // Added this method
}