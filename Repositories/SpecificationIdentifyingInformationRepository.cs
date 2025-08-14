// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.DTOs;
using RegistryApi.Helpers;
using RegistryApi.Models;
using System.Linq;

namespace RegistryApi.Repositories;

public class SpecificationIdentifyingInformationRepository(RegistryDbContext context)
    : GenericRepository<SpecificationIdentifyingInformation>(context), ISpecificationIdentifyingInformationRepository
{
    public async Task<PagedList<SpecificationIdentifyingInformation>> GetAllPaginatedAsync(PaginationParams paginationParams, bool includeSubmittedAndInProgress = false)
    {
        var query = _dbSet.Include(s => s.UserGroup).AsNoTracking();

        if (!includeSubmittedAndInProgress)
        {
            query = query.Where(s => s.RegistrationStatus == null || (s.RegistrationStatus.ToLower() != "submitted" && s.RegistrationStatus.ToLower() != "in progress"));
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
        {
            string searchTermLower = paginationParams.SearchTerm.ToLower();
            query = query.Where(s =>
                (s.SpecificationName != null && s.SpecificationName.ToLower().Contains(searchTermLower)) ||
                (s.Purpose != null && s.Purpose.ToLower().Contains(searchTermLower)) ||
                (s.Sector != null && s.Sector.ToLower().Contains(searchTermLower))
            );
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.ExtensionBusinessTermId))
        {
            string extensionBusinessTermIdLower = paginationParams.ExtensionBusinessTermId.ToLower();

            var identityIdsQuery = _context.SpecificationExtensionComponents
                                           .Where(sec => sec.BusinessTermID.ToLower().Contains(extensionBusinessTermIdLower))
                                           .Select(sec => sec.IdentityID)
                                           .Distinct();

            query = query.Where(s => identityIdsQuery.Contains(s.IdentityID));
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.CoreBusinessTermId))
        {
            string coreBusinessTermIdLower = paginationParams.CoreBusinessTermId.ToLower();

            var identityIdsQuery = _context.SpecificationCores
                                           .Where(sc => sc.BusinessTermID.ToLower().Contains(coreBusinessTermIdLower))
                                           .Select(sc => sc.IdentityID)
                                           .Distinct();

            query = query.Where(s => identityIdsQuery.Contains(s.IdentityID));
        }

        // --- NEWLY ADDED BLOCK ---
        if (!string.IsNullOrWhiteSpace(paginationParams.AddReqBusinessTermID))
        {
            string addReqBusinessTermIdLower = paginationParams.AddReqBusinessTermID.ToLower();

            var identityIdsQuery = _context.AdditionalRequirements
                                           .Where(ar => ar.BusinessTermID.ToLower().Contains(addReqBusinessTermIdLower))
                                           .Select(ar => ar.IdentityID)
                                           .Distinct();

            query = query.Where(s => identityIdsQuery.Contains(s.IdentityID));
        }
        // --- END OF NEW BLOCK ---

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

        if (!string.IsNullOrWhiteSpace(paginationParams.SortBy))
        {
            bool isDescending = paginationParams.SortOrder?.ToUpper() == "DESC";
            query = paginationParams.SortBy.ToLowerInvariant() switch
            {
                "specificationname" => isDescending ? query.OrderByDescending(s => s.SpecificationName) : query.OrderBy(s => s.SpecificationName),
                "purpose" => isDescending ? query.OrderByDescending(s => s.Purpose) : query.OrderBy(s => s.Purpose),
                "sector" => isDescending ? query.OrderByDescending(s => s.Sector) : query.OrderBy(s => s.Sector),
                "country" => isDescending ? query.OrderByDescending(s => s.Country) : query.OrderBy(s => s.Country),
                "specificationtype" => isDescending ? query.OrderByDescending(s => s.SpecificationType) : query.OrderBy(s => s.SpecificationType),
                "modifieddate" => isDescending ? query.OrderByDescending(s => s.ModifiedDate) : query.OrderBy(s => s.ModifiedDate),
                "createddate" => isDescending ? query.OrderByDescending(s => s.CreatedDate) : query.OrderBy(s => s.CreatedDate),
                "specificationidentifier" => isDescending ? query.OrderByDescending(s => s.SpecificationIdentifier) : query.OrderBy(s => s.SpecificationIdentifier),
                _ => query.OrderByDescending(s => s.ModifiedDate),
            };
        }
        else
        {
            query = query.OrderByDescending(s => s.ModifiedDate);
        }

        return await PagedList<SpecificationIdentifyingInformation>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<IEnumerable<SpecificationIdentifyingInformation>> GetAllAsync(bool includeSubmittedAndInProgress = false)
    {
        var query = _dbSet.Include(s => s.UserGroup).AsNoTracking();

        if (!includeSubmittedAndInProgress)
        {
            query = query.Where(s => s.RegistrationStatus == null || (s.RegistrationStatus.ToLower() != "submitted" && s.RegistrationStatus.ToLower() != "in progress"));
        }

        query = query.OrderByDescending(s => s.ModifiedDate);

        return await query.ToListAsync();
    }


    public async Task<IEnumerable<SpecificationIdentifyingInformation>> GetByUserGroupIdAsync(int userGroupId)
    {
        return await _dbSet
            .Where(s => s.UserGroupID == userGroupId)
            .Include(s => s.UserGroup)
            .AsNoTracking()
            .OrderBy(s => s.SpecificationIdentifier)
            .ToListAsync();
    }

    public override async Task<SpecificationIdentifyingInformation?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(s => s.UserGroup)
            .FirstOrDefaultAsync(s => s.IdentityID == id);
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