// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

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
