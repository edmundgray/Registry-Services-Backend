using System;
// Add this using statement for BCrypt.Net-Next
using BCryptNet = BCrypt.Net.BCrypt; // Alias to avoid naming conflicts if any

namespace RegistryApi.Services
{
    // Updated to use BCrypt.Net-Next
    public class PasswordHasher : IPasswordHasher
    {
        // Work factor for BCrypt. Higher is more secure but slower.
        // Common values are between 10 and 12. Adjust as needed for your security/performance balance.
        private const int WorkFactor = 12;

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            // BCrypt.Net-Next automatically handles salt generation and embedding it in the hash.
            return BCryptNet.HashPassword(password, WorkFactor);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
            {
                return false;
            }

            try
            {
                // BCrypt.Net-Next Verify method handles extracting the salt from hashedPassword.
                return BCryptNet.Verify(providedPassword, hashedPassword);
            }
            catch (BCrypt.Net.SaltParseException ex)
            {
                // Log this exception: it means the hash format is incorrect.
                // Consider how to handle this - for now, returning false.
                Console.WriteLine($"Error verifying password: SaltParseException - {ex.Message}"); // Replace with proper logging
                return false;
            }
            catch (Exception ex) // Catch other potential exceptions during verification
            {
                Console.WriteLine($"Error verifying password: {ex.Message}"); // Replace with proper logging
                return false;
            }
        }
    }
}