// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using RegistryApi.Models;
using RegistryApi.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RegistryApi.Repositories
{
    public interface IUserGroupRepository : IGenericRepository<UserGroup>
    {
        Task<UserGroup?> GetByNameAsync(string groupName);
        Task<IEnumerable<UserGroupDto>> GetAllWithCountsAsync();
    }
}
