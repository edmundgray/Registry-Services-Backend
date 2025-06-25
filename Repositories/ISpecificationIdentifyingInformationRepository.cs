using RegistryApi.Models;
using RegistryApi.DTOs; // For PaginationParams
using RegistryApi.Helpers; // For PagedList

namespace RegistryApi.Repositories;


public interface ISpecificationIdentifyingInformationRepository : IGenericRepository<SpecificationIdentifyingInformation>
{

    Task<PagedList<SpecificationIdentifyingInformation>> GetAllPaginatedAsync(PaginationParams paginationParams, bool includeSubmitted = false);
    Task<PagedList<SpecificationIdentifyingInformation>> GetByUserGroupIdPaginatedAsync(int userGroupId, PaginationParams paginationParams); 
    Task<bool> HasCoreElementsAsync(int id);
    Task<bool> HasExtensionComponentsAsync(int id);
    Task<bool> ExistsAsync(int id);
}
