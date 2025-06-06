using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.DTOs;
using RegistryApi.Helpers;
using RegistryApi.Models;
using AutoMapper; // Required for mapping

namespace RegistryApi.Repositories;

public class ExtensionComponentsModelHeaderRepository(RegistryDbContext context, IMapper mapper)
    : GenericRepository<ExtensionComponentsModelHeader>(context), IExtensionComponentsModelHeaderRepository
{
    private readonly IMapper _mapper = mapper;

    public async Task<PagedList<ExtensionComponentsModelHeaderDto>> GetAllAsync(PaginationParams paginationParams)
    {
        var query = _dbSet
            .AsNoTracking()
            .OrderBy(h => h.ID); // Order by Name for example
            

        var pagedEntities = await PagedList<ExtensionComponentsModelHeader>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);

        // Map the paged entities to DTOs
        var pagedDtos = new PagedList<ExtensionComponentsModelHeaderDto>(
            _mapper.Map<List<ExtensionComponentsModelHeaderDto>>(pagedEntities.Items),
            pagedEntities.TotalCount,
            pagedEntities.PageNumber,
            pagedEntities.PageSize
        );
        return pagedDtos;
    }

    // Implementing the new GetByIdAsync method for string ID
    public async Task<ExtensionComponentsModelHeader?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(h => h.ID == id);
    }
}