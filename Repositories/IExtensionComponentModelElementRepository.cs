// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using RegistryApi.DTOs;
using RegistryApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RegistryApi.Repositories
{
    public interface IExtensionComponentModelElementRepository
    {
        Task<IEnumerable<ExtensionComponentModelElementDto>> GetByExtensionComponentIdAsync(string extensionComponentId);
        Task<ExtensionComponentModelElementDto> GetByIdAsync(int id);
        Task<IEnumerable<ExtensionComponentModelElementDto>> GetAllAsync();
        Task AddAsync(ExtensionComponentModelElement entity);
        Task UpdateAsync(ExtensionComponentModelElement entity);
        Task DeleteAsync(int id);
    }
}
