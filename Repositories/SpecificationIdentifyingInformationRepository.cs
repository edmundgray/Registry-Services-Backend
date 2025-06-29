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
    public async Task<PagedList<SpecificationIdentifyingInformation>> GetAllPaginatedAsync(PaginationParams paginationParams, bool includeSubmittedAndInProgress = false)
    {
        var query = _dbSet.AsNoTracking();

        if (!includeSubmittedAndInProgress)
        {
            var excludedStatuses = new List<string> { "submitted", "in Progress" };
            query = query.Where(s => s.RegistrationStatus == null || (s.RegistrationStatus.ToLower() != "submitted" && s.RegistrationStatus.ToLower() != "in progress"));
        }

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

        // Specific Field Filters
        if (!string.IsNullOrWhiteSpace(paginationParams.SpecificationType))
        {
            query = query.Where(s => s.SpecificationType != null && s.SpecificationType.ToLower() == paginationParams.SpecificationType.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.Sector))
        {
            query = query.Where(s => s.Sector != null && s.Sector.ToLower() == paginationParams.Sector.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.Country))
        {
            query = query.Where(s => s.Country != null && s.Country.ToLower() == paginationParams.Country.ToLower());
        }

        // Sorting Logic
        if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
        {
            bool isDescending = paginationParams.SortOrder?.ToUpper() == "DESC";

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
                case "country":
                    query = isDescending ? query.OrderByDescending(s => s.Country) : query.OrderBy(s => s.Country);
                    break;
                case "specificationtype":
                    query = isDescending ? query.OrderByDescending(s => s.SpecificationType) : query.OrderBy(s => s.SpecificationType);
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
                default:
                    query = query.OrderByDescending(s => s.ModifiedDate);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(s => s.ModifiedDate);
        }

        return await PagedList<SpecificationIdentifyingInformation>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
    }

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