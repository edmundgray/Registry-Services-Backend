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
        var query = _dbSet.AsNoTracking();

        // Generic SearchTerm Filter for SpecificationName, Purpose, and Sector
        if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
        {
            string searchTermLower = paginationParams.SearchTerm.ToLower();
            query = query.Where(s =>
                (s.SpecificationName != null && s.SpecificationName.ToLower().Contains(searchTermLower)) ||
                (s.Purpose != null && s.Purpose.ToLower().Contains(searchTermLower)) ||
                (s.Sector != null && s.Sector.ToLower().Contains(searchTermLower))
            );
        }

        // Sorting Logic
        if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
        {
            bool isDescending = paginationParams.SortOrder?.ToUpper() == "DESC";
            
            // Ensure property names in SortBy match model properties exactly (case-insensitive for switch).
            switch (paginationParams.SortBy.ToLowerInvariant())
            {
                case "specificationname":
                    query = isDescending ? query.OrderByDescending(s => s.SpecificationName) : query.OrderBy(s => s.SpecificationName);
                    break;
                case "purpose":
                    query = isDescending ? query.OrderByDescending(s => s.Purpose) : query.OrderBy(s => s.Purpose);
                    break;
                case "sector":
                    query = isDescending ? query.OrderByDescending(s => s.Sector) : query.OrderBy(s => s.Sector);
                    break;
                case "modifieddate":
                    query = isDescending ? query.OrderByDescending(s => s.ModifiedDate) : query.OrderBy(s => s.ModifiedDate);
                    break;
                case "createddate":
                    query = isDescending ? query.OrderByDescending(s => s.CreatedDate) : query.OrderBy(s => s.CreatedDate);
                    break;
                case "specificationidentifier":
                    query = isDescending ? query.OrderByDescending(s => s.SpecificationIdentifier) : query.OrderBy(s => s.SpecificationIdentifier);
                    break;
                default: // Default sort if SortBy is provided but not matched or if it's an unsupported field
                    query = query.OrderByDescending(s => s.ModifiedDate);
                    break;
            }
        }
        else
        {
            // Default Sort if SortBy is not provided
            query = query.OrderByDescending(s => s.ModifiedDate);
        }

        return await PagedList<SpecificationIdentifyingInformation>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<PagedList<SpecificationIdentifyingInformation>> GetByUserGroupIdPaginatedAsync(int userGroupId, PaginationParams paginationParams)
    {
        // This method remains unchanged and does not use SearchTerm, SortBy, or SortOrder from paginationParams.
        // It retains its original filtering by UserGroupID and sorting by SpecificationIdentifier.
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
