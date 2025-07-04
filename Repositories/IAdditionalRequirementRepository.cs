using RegistryApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RegistryApi.Repositories
{
    public interface IAdditionalRequirementRepository : IGenericRepository<AdditionalRequirement>
    {
        Task<AdditionalRequirement?> GetByIdAsync(int specificationId, string businessTermId);
        Task<IEnumerable<AdditionalRequirement>> GetAllBySpecificationIdAsync(int specificationId);
    }
}
