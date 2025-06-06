using RegistryApi.Models;
using RegistryApi.DTOs;
using RegistryApi.Helpers;

namespace RegistryApi.Repositories;

/// <summary>
/// Interface for the ExtensionComponentModelElement repository.
/// </summary>
public interface IExtensionComponentModelElementRepository : IGenericRepository<ExtensionComponentModelElement>
{
    /// <summary>
    /// Retrieves a paginated list of ExtensionComponentModelElement items filtered by ExtensionComponentID.
    /// </summary>
    /// <param name="extensionComponentId">The ID of the parent extension component.</param>
    /// <param name="paginationParams">Parameters for pagination.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paged list of ExtensionComponentModelElementDto.</returns>
    Task<PagedList<ExtensionComponentModelElementDto>> GetByExtensionComponentIdAsync(string extensionComponentId, PaginationParams paginationParams);
}
