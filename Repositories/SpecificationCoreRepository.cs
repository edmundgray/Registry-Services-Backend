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
    public async Task<IEnumerable<SpecificationCore>> GetAllBySpecificationIdAsync(int specificationId)
    {
        return await _dbSet
           .Where(sc => sc.IdentityID == specificationId)
           .Include(sc => sc.CoreInvoiceModel)
           .AsNoTracking()
           .OrderBy(sc => sc.CoreInvoiceModel.RowPos)
           .ToListAsync();
    }
    public async Task<PagedList<SpecificationCore>> GetBySpecificationIdPaginatedAsync(int specificationId, PaginationParams paginationParams)
    {
        var query = _dbSet
           .Where(sc => sc.IdentityID == specificationId)
           .Include(sc => sc.CoreInvoiceModel) // Eagerly load the CoreInvoiceModel
           .AsNoTracking()
           .OrderBy(sc => sc.CoreInvoiceModel.RowPos); // Order by RowPos as specified in CoreInvoiceModel.cs

        return await PagedList<SpecificationCore>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<SpecificationCore?> GetByIdAndSpecificationIdAsync(int coreElementId, int specificationId)
    {
        // Find by composite logical key (PK + Parent FK)
        // Include CoreInvoiceModel to get the related details for the DTO mapping
        return await _dbSet
                      .Include(sc => sc.CoreInvoiceModel) // Eagerly load the CoreInvoiceModel
                      .FirstOrDefaultAsync(sc => sc.EntityID == coreElementId && sc.IdentityID == specificationId);
    }

    public async Task<bool> CoreInvoiceModelExistsAsync(string businessTermId)
    {
        // Access context directly for related table checks
        return await _context.CoreInvoiceModels.AnyAsync(cim => cim.ID == businessTermId);
    }
}
