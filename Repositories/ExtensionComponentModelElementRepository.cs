using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.DTOs;
using RegistryApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryApi.Repositories
{
    public class ExtensionComponentModelElementRepository : IExtensionComponentModelElementRepository
    {
        private readonly RegistryDbContext _context;
        private readonly IMapper _mapper;

        public ExtensionComponentModelElementRepository(RegistryDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ExtensionComponentModelElementDto>> GetByExtensionComponentIdAsync(string extensionComponentId)
        {
            return await _context.ExtensionComponentModelElements
                                 .Where(e => e.ExtensionComponentID == extensionComponentId)
                                 .AsNoTracking()
                                 .ProjectTo<ExtensionComponentModelElementDto>(_mapper.ConfigurationProvider)
                                 .ToListAsync();
        }

        public async Task<ExtensionComponentModelElementDto> GetByIdAsync(int id)
        {
            var element = await _context.ExtensionComponentModelElements
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(e => e.EntityID == id);
            return _mapper.Map<ExtensionComponentModelElementDto>(element);
        }

        public async Task<IEnumerable<ExtensionComponentModelElementDto>> GetAllAsync()
        {
            return await _context.ExtensionComponentModelElements
                                 .AsNoTracking()
                                 .ProjectTo<ExtensionComponentModelElementDto>(_mapper.ConfigurationProvider)
                                 .ToListAsync();
        }

        public Task AddAsync(ExtensionComponentModelElement entity)
        {
            // Implementation for adding an entity
            throw new System.NotImplementedException();
        }

        public Task UpdateAsync(ExtensionComponentModelElement entity)
        {
            // Implementation for updating an entity
            throw new System.NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            // Implementation for deleting an entity
            throw new System.NotImplementedException();
        }
    }
}
