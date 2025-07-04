using Microsoft.EntityFrameworkCore;
using RegistryApi.Data;
using RegistryApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryApi.Repositories
{
    public class AdditionalRequirementRepository : GenericRepository<AdditionalRequirement>, IAdditionalRequirementRepository
    {
        public AdditionalRequirementRepository(RegistryDbContext context) : base(context)
        {
        }

        public async Task<AdditionalRequirement?> GetByIdAsync(int specificationId, string businessTermId)
        {
            return await _dbSet.AsNoTracking()
                               .FirstOrDefaultAsync(ar => ar.IdentityID == specificationId && ar.BusinessTermID == businessTermId);
        }

        public async Task<IEnumerable<AdditionalRequirement>> GetAllBySpecificationIdAsync(int specificationId)
        {
            return await _dbSet.AsNoTracking()
                               .Where(ar => ar.IdentityID == specificationId)
                               .OrderBy(ar => ar.RowPos)
                               .ToListAsync();
        }
    }
}
