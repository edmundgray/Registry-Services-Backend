using RegistryApi.Models;
using RegistryApi.DTOs;
using RegistryApi.Helpers;

namespace RegistryApi.Repositories;

public interface ISpecificationCoreRepository : IGenericRepository<SpecificationCore>
{
     Task<PagedList<SpecificationCore>> GetBySpecificationIdPaginatedAsync(int specificationId, PaginationParams paginationParams);
     Task<SpecificationCore?> GetByIdAndSpecificationIdAsync(int coreElementId, int specificationId);
     Task<bool> CoreInvoiceModelExistsAsync(string businessTermId);
}
