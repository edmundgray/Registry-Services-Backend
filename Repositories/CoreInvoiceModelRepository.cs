using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.DTOs;
using RegistryApi.Helpers;
using RegistryApi.Models;
using AutoMapper; // Required for mapping in repository if done here, but usually done in service

namespace RegistryApi.Repositories;

public class CoreInvoiceModelRepository(RegistryDbContext context, IMapper mapper)
    : GenericRepository<CoreInvoiceModel>(context), ICoreInvoiceModelRepository
{
    private readonly IMapper _mapper = mapper; // Inject IMapper

    public async Task<PagedList<CoreInvoiceModelDto>> GetAllAsync(PaginationParams paginationParams)
    {
        var query = _dbSet
            .AsNoTracking()
            .OrderBy(cim => cim.RowPos) // Order by RowPos as specified in CoreInvoiceModel.cs
            .ThenBy(cim => cim.ID); // Secondary sort by ID

        var pagedEntities = await PagedList<CoreInvoiceModel>.CreateAsync(query, paginationParams.PageNumber, paginationParams.PageSize);

        // Map the paged entities to DTOs
        var pagedDtos = new PagedList<CoreInvoiceModelDto>(
            _mapper.Map<List<CoreInvoiceModelDto>>(pagedEntities.Items),
            pagedEntities.TotalCount,
            pagedEntities.PageNumber,
            pagedEntities.PageSize
        );
        return pagedDtos;
    }
}