using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.DTOs;
using RegistryApi.Helpers;
using RegistryApi.Models;
using AutoMapper; // Required for mapping

namespace RegistryApi.Repositories;

public class ExtensionComponentModelElementRepository(RegistryDbContext context, IMapper mapper)
    : GenericRepository<ExtensionComponentModelElement>(context), IExtensionComponentModelElementRepository
{
    private readonly IMapper _mapper = mapper;

    public async Task<PagedList<ExtensionComponentModelElementDto>> GetByExtensionComponentIdAsync(string extensionComponentId, PaginationParams paginationParams)
    {
        var query = _dbSet
            .Where(e => e.ExtensionComponentID == extensionComponentId)
            .AsNoTracking()
            .OrderBy(e => e.BusinessTerm) // Order by BusinessTerm for example
            .ThenBy(e => e.BusinessTermID); // Secondary sort

        var pagedEntities = await PagedList<ExtensionComponentModelElement>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);

        // Map the paged entities to DTOs
        var pagedDtos = new PagedList<ExtensionComponentModelElementDto>(
            _mapper.Map<List<ExtensionComponentModelElementDto>>(pagedEntities.Items),
            pagedEntities.TotalCount,
            pagedEntities.PageNumber,
            pagedEntities.PageSize
        );
        return pagedDtos;
    }
}