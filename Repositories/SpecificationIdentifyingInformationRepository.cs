using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.DTOs;
using RegistryApi.Helpers;
using RegistryApi.Models;

namespace RegistryApi.Repositories;

// Using primary constructor
public class SpecificationIdentifyingInformationRepository(RegistryDbContext context)
    : GenericRepository<SpecificationIdentifyingInformation>(context), ISpecificationIdentifyingInformationRepository
{
    public async Task<PagedList<SpecificationIdentifyingInformation>> GetAllPaginatedAsync(PaginationParams paginationParams)
    {
        var query = _dbSet.AsNoTracking()
                          .OrderBy(s => s.SpecificationIdentifier); // Example ordering

        return await PagedList<SpecificationIdentifyingInformation>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
    }

    // GetByIdAsync from GenericRepository is sufficient for fetching the header

    public async Task<bool> HasCoreElementsAsync(int id)
    {
        // Access context directly for related table checks
        return await _context.SpecificationCores.AnyAsync(sc => sc.IdentityID == id);
    }

    public async Task<bool> HasExtensionComponentsAsync(int id)
    {
         return await _context.SpecificationExtensionComponents.AnyAsync(sec => sec.IdentityID == id);
    }

     public async Task<bool> ExistsAsync(int id)
    {
        // Use the primary key (IdentityID) for existence check
        return await _dbSet.AnyAsync(e => e.IdentityID == id);
    }
}
