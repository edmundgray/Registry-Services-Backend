using RegistryApi.Models;
using RegistryApi.DTOs; // For PaginationParams
using RegistryApi.Helpers; // For PagedList

namespace RegistryApi.Repositories;

public interface ISpecificationIdentifyingInformationRepository : IGenericRepository<SpecificationIdentifyingInformation>
{
    Task<PagedList<SpecificationIdentifyingInformation>> GetAllPaginatedAsync(PaginationParams paginationParams);
    // Removed GetByIdWithDetailsAsync - loading details with pagination is better handled in the service layer
    Task<bool> HasCoreElementsAsync(int id);
    Task<bool> HasExtensionComponentsAsync(int id);
    Task<bool> ExistsAsync(int id);
}
