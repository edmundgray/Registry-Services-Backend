using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.DTOs;
using RegistryApi.Helpers;
using RegistryApi.Models;

namespace RegistryApi.Repositories;

// Using primary constructor
public class SpecificationCoreRepository(RegistryDbContext context)
    : GenericRepository<SpecificationCore>(context), ISpecificationCoreRepository
{
    public async Task<PagedList<SpecificationCore>> GetBySpecificationIdPaginatedAsync(int specificationId, PaginationParams paginationParams)
    {
         var query = _dbSet
            .Where(sc => sc.IdentityID == specificationId)
            .AsNoTracking()
            .OrderBy(sc => sc.BusinessTermID); // Example ordering

        return await PagedList<SpecificationCore>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
    }

     public async Task<SpecificationCore?> GetByIdAndSpecificationIdAsync(int coreElementId, int specificationId)
     {
          // Find by composite logical key (PK + Parent FK)
          return await _dbSet.FirstOrDefaultAsync(sc => sc.EntityID == coreElementId && sc.IdentityID == specificationId);
     }

    public async Task<bool> CoreInvoiceModelExistsAsync(string businessTermId)
    {
        // Access context directly for related table checks
        return await _context.CoreInvoiceModels.AnyAsync(cim => cim.ID == businessTermId);
    }
}
