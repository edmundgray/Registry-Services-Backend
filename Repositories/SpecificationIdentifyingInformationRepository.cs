// In Repositories/SpecificationIdentifyingInformationRepository.cs
using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.DTOs; // Ensure this using is present if PaginationParams is here
using RegistryApi.Helpers; // Or ensure this using is present if PaginationParams is here
using RegistryApi.Models;
using System.Linq; // Required for Where clause

namespace RegistryApi.Repositories;

public class SpecificationIdentifyingInformationRepository(RegistryDbContext context)
    : GenericRepository<SpecificationIdentifyingInformation>(context), ISpecificationIdentifyingInformationRepository
{
    public async Task<PagedList<SpecificationIdentifyingInformation>> GetAllPaginatedAsync(PaginationParams paginationParams)
    {
        var query = _dbSet.AsNoTracking()
                          .OrderBy(s => s.SpecificationIdentifier);
        return await PagedList<SpecificationIdentifyingInformation>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
    }

    // New Method Implementation
    public async Task<PagedList<SpecificationIdentifyingInformation>> GetByUserGroupIdPaginatedAsync(int userGroupId, PaginationParams paginationParams)
    {
        var query = _dbSet
            .Where(s => s.UserGroupID == userGroupId)
            .AsNoTracking()
            .OrderBy(s => s.SpecificationIdentifier);
        return await PagedList<SpecificationIdentifyingInformation>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<bool> HasCoreElementsAsync(int id)
    {
        return await _context.SpecificationCores.AnyAsync(sc => sc.IdentityID == id);
    }

    public async Task<bool> HasExtensionComponentsAsync(int id)
    {
        return await _context.SpecificationExtensionComponents.AnyAsync(sec => sec.IdentityID == id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(e => e.IdentityID == id);
    }
}
