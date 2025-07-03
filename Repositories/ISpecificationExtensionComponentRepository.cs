using RegistryApi.Models;
using RegistryApi.DTOs;
using RegistryApi.Helpers;

namespace RegistryApi.Repositories;

public interface ISpecificationExtensionComponentRepository : IGenericRepository<SpecificationExtensionComponent>
{
     Task<PagedList<SpecificationExtensionComponent>> GetBySpecificationIdPaginatedAsync(int specificationId, PaginationParams paginationParams);
     Task<SpecificationExtensionComponent?> GetByIdAndSpecificationIdAsync(int extensionElementId, int specificationId);
     Task<bool> ExtensionElementExistsAsync(string extensionComponentId, string businessTermId);

    Task<IEnumerable<SpecificationExtensionComponent>> GetAllBySpecificationIdAsync(int specificationId);
}
