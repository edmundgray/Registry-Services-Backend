using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.DTOs;
using RegistryApi.Helpers;
using RegistryApi.Models;

namespace RegistryApi.Repositories;

// Using primary constructor
public class SpecificationExtensionComponentRepository(RegistryDbContext context)
    : GenericRepository<SpecificationExtensionComponent>(context), ISpecificationExtensionComponentRepository
{
    public async Task<IEnumerable<SpecificationExtensionComponent>> GetAllBySpecificationIdAsync(int specificationId)
    {
        return await _dbSet
            .Where(sec => sec.IdentityID == specificationId)
            .Include(sec => sec.ExtensionComponentModelElement)
            .AsNoTracking()
            .OrderBy(sec => sec.ExtensionComponentID).ThenBy(sec => sec.BusinessTermID)
            .ToListAsync();
    }
    public async Task<PagedList<SpecificationExtensionComponent>> GetBySpecificationIdPaginatedAsync(int specificationId, PaginationParams paginationParams)
    {
         var query = _dbSet
            .Where(sec => sec.IdentityID == specificationId)
            .Include(sec => sec.ExtensionComponentModelElement) // Added Include statement
            .AsNoTracking()
            .OrderBy(sec => sec.ExtensionComponentID).ThenBy(sec => sec.BusinessTermID);

        return await PagedList<SpecificationExtensionComponent>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
    }

     public async Task<SpecificationExtensionComponent?> GetByIdAndSpecificationIdAsync(int extensionElementId, int specificationId)
    {
         // Find by composite logical key (PK + Parent FK)
         return await _dbSet.FirstOrDefaultAsync(sec => sec.EntityID == extensionElementId && sec.IdentityID == specificationId);
    }

    public async Task<bool> ExtensionElementExistsAsync(string extensionComponentId, string businessTermId)
    {
        // Check against the ExtensionComponentModelElements table using the composite key
        return await _context.ExtensionComponentModelElements
                     .AnyAsync(ece => ece.ExtensionComponentID == extensionComponentId && ece.BusinessTermID == businessTermId);
    }
}
