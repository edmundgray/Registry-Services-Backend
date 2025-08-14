// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

namespace RegistryApi.Services
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);
    }
}
