using System;
using System.Security.Cryptography; // Required for a slightly better placeholder than plain text
using System.Text;

namespace RegistryApi.Services
{
    // WARNING: This is a placeholder implementation for password hashing.
    // It is NOT cryptographically secure and MUST NOT be used in a production environment.
    // Replace this with a robust library like BCrypt.Net or ASP.NET Core Identity's password hashing.
    public class PasswordHasher : IPasswordHasher
    {
        private const string Salt = "REGISTRY_API_SALT_VALUE_REPLACE_THIS"; // Example salt, should be unique and ideally per-user in a real system

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            // Placeholder: Simple SHA256 hash with a static salt.
            // Real systems use algorithms like BCrypt or Argon2 which are designed for password hashing.
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + Salt;
                var bytes = Encoding.UTF8.GetBytes(saltedPassword);
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
            {
                return false;
            }
            // Verify by re-hashing the provided password with the same salt and comparing.
            string hashedProvidedPassword = HashPassword(providedPassword);
            return hashedPassword == hashedProvidedPassword;
        }
    }
}
