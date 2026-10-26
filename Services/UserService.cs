// SPDX-FileCopyrightText: 2025 CEN - European Committee for Standardization
// SPDX-License-Identifier: EUPL-1.2

using AutoMapper;
using RegistryApi.DTOs;
using RegistryApi.Models;
using RegistryApi.Repositories;
using RegistryApi.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
// Add these using statements for JWT generation
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration; // For IConfiguration
using Microsoft.IdentityModel.Tokens;

namespace RegistryApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserGroupRepository _userGroupRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration; // <-- Add IConfiguration

        public UserService(
            IUserRepository userRepository,
            IUserGroupRepository userGroupRepository,
            IMapper mapper,
            IPasswordHasher passwordHasher,
            ILogger<UserService> logger,
            IConfiguration configuration) // <-- Inject IConfiguration
        {
            _userRepository = userRepository;
            _userGroupRepository = userGroupRepository;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _configuration = configuration; // <-- Assign IConfiguration
        }

        public async Task<(ServiceResult Status, UserTokenDto? TokenDto, string? ErrorMessage)> AuthenticateUserAsync(UserLoginDto loginDto)
        {
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
            if (user == null || !user.IsActive || !_passwordHasher.VerifyPassword(user.PasswordHash, loginDto.Password))
            {
                return (ServiceResult.Unauthorized, null, "Invalid username or password.");
            }

            await UpdateLastLoginDateAsync(user.UserID);

            var (tokenString, expirationMinutes) = CreateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            var userGroup = user.UserGroupID.HasValue ? await _userGroupRepository.GetByIdAsync(user.UserGroupID.Value) : null;

            var tokenDto = new UserTokenDto(
                 token: tokenString,
                 refreshToken: refreshToken,
                 expiresIn: expirationMinutes * 60,
                 userId: user.UserID,
                 username: user.Username,
                 role: user.Role,
                 userGroupID: user.UserGroupID,
                 groupName: userGroup?.GroupName
            );

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Example: Refresh token expires in 7 days
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();


            return (ServiceResult.Success, tokenDto, null);
        }
        public async Task<(ServiceResult Status, UserTokenDto? TokenDto, string? ErrorMessage)> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            ClaimsPrincipal principal;
            try
            {
                principal = GetPrincipalFromExpiredToken(refreshTokenDto.accessToken);
            }
            catch (Exception ex) when (ex is SecurityTokenException || ex is ArgumentException)
            {
                return (ServiceResult.Unauthorized, null, "Invalid token.");
            }

            if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return (ServiceResult.Unauthorized, null, "Invalid token.");
            }
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || !user.IsActive || user.RefreshToken != refreshTokenDto.refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return (ServiceResult.Unauthorized, null, "Invalid refresh token or refresh token expired.");
            }

            // Claims come from the current database record, so role/group changes and deactivation
            // take effect at the next refresh rather than surviving for the refresh token's lifetime.
            var (newAccessToken, expirationMinutes) = CreateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            var userGroup = user.UserGroupID.HasValue ? await _userGroupRepository.GetByIdAsync(user.UserGroupID.Value) : null;

            var tokenDto = new UserTokenDto(
                token: newAccessToken,
                refreshToken: newRefreshToken,
                expiresIn: expirationMinutes * 60,
                userId: user.UserID,
                username: user.Username,
                role: user.Role,
                userGroupID: user.UserGroupID,
                groupName: userGroup?.GroupName
            );

            return (ServiceResult.Success, tokenDto, null);
        }

        public async Task<ServiceResult> RevokeRefreshTokenAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return ServiceResult.NotFound;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            _userRepository.Update(user);
            return await _userRepository.SaveChangesAsync() ? ServiceResult.Success : ServiceResult.BadRequest;
        }

        private (string Token, int ExpirationMinutes) CreateAccessToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expirationMinutes = int.TryParse(jwtSettings["ExpirationInMinutes"], out var exp) ? exp : 60;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };
            if (user.UserGroupID.HasValue)
            {
                claims.Add(new Claim("UserGroupId", user.UserGroupID.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                SigningCredentials = creds,
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return (tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor)), expirationMinutes);
        }

        private string GenerateRefreshToken()
        {
            // Simple refresh token generation. In a real-world scenario, you might want to use a more secure random string generation.
            return Guid.NewGuid().ToString();
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = _configuration["JwtSettings:Audience"],
                ValidateIssuer = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!)),
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                ValidateLifetime = false // the access token is expected to be expired; the refresh token is the credential
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }


        // ... (other methods like RegisterUserAsync, UpdateLastLoginDateAsync, etc. remain the same)
        // Ensure other methods are present as per your existing UserService.cs
        public async Task<(ServiceResult Status, UserDto? Dto, string? ErrorMessage)> RegisterUserAsync(UserCreateDto createDto)
        {
            if (await _userRepository.GetByUsernameAsync(createDto.Username) != null)
            {
                return (ServiceResult.Conflict, null, "Username already exists.");
            }
            if (await _userRepository.GetByEmailAsync(createDto.Email) != null)
            {
                return (ServiceResult.Conflict, null, "Email already exists.");
            }

            if (createDto.UserGroupID.HasValue && await _userGroupRepository.GetByIdAsync(createDto.UserGroupID.Value) == null)
            {
                return (ServiceResult.RefNotFound, null, "Specified UserGroup does not exist.");
            }

            var user = _mapper.Map<User>(createDto);
            user.PasswordHash = _passwordHasher.HashPassword(createDto.Password);

            if (string.IsNullOrWhiteSpace(user.Role))
            {
                user.Role = "User";
            }
            else if (user.Role != "Admin" && user.Role != "User")
            {
                return (ServiceResult.BadRequest, null, "Role must be 'Admin' or 'User'.");
            }
            user.CreatedDate = DateTime.UtcNow;
            user.IsActive = true;

            await _userRepository.AddAsync(user);
            if (!await _userRepository.SaveChangesAsync())
            {
                _logger.LogError("Failed to save new user during registration for username {Username}", createDto.Username);
                return (ServiceResult.BadRequest, null, "Could not register user due to a save error.");
            }

            return (ServiceResult.Success, _mapper.Map<UserDto>(user), null);
        }

        public async Task<(ServiceResult Status, UserDto? Dto)> UpdateUserAsync(int userId, UserUpdateDto updateDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return (ServiceResult.NotFound, null);
            }

            if (user.Email != updateDto.Email && await _userRepository.GetByEmailAsync(updateDto.Email) != null)
            {
                return (ServiceResult.Conflict, null);
            }

            if (updateDto.UserGroupID.HasValue && await _userGroupRepository.GetByIdAsync(updateDto.UserGroupID.Value) == null)
            {
                return (ServiceResult.RefNotFound, null);
            }

            _mapper.Map(updateDto, user);

            _userRepository.Update(user);
            if (!await _userRepository.SaveChangesAsync())
            {
                _logger.LogError("Failed to save updated user with ID {UserId}", userId);
                return (ServiceResult.BadRequest, null);
            }
            return (ServiceResult.Success, _mapper.Map<UserDto>(user));
        }


        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(PaginationParams paginationParams)
        {
            // This should ideally use a paginated repository method
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }
        public async Task<IEnumerable<UserDto>> GetUsersByGroupAsync(int groupId)
        {
            var users = await _userRepository.GetUsersByGroupIdAsync(groupId);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }
        public async Task<ServiceResult> AssignUserToGroupAsync(int userId, int? userGroupId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return ServiceResult.NotFound;

            if (userGroupId.HasValue)
            {
                var group = await _userGroupRepository.GetByIdAsync(userGroupId.Value);
                if (group == null) return ServiceResult.RefNotFound;
            }

            user.UserGroupID = userGroupId;
            _userRepository.Update(user);
            return await _userRepository.SaveChangesAsync() ? ServiceResult.Success : ServiceResult.BadRequest;
        }

        public async Task<ServiceResult> ChangeUserRoleAsync(int userId, string newRole)
        {
            if (newRole != "Admin" && newRole != "User") // Basic role validation
            {
                return ServiceResult.BadRequest;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return ServiceResult.NotFound;

            user.Role = newRole;
            _userRepository.Update(user);
            return await _userRepository.SaveChangesAsync() ? ServiceResult.Success : ServiceResult.BadRequest;
        }

        public async Task<ServiceResult> UpdateLastLoginDateAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return ServiceResult.NotFound;

            user.LastLoginDate = DateTime.UtcNow;
            _userRepository.Update(user);
            return await _userRepository.SaveChangesAsync() ? ServiceResult.Success : ServiceResult.BadRequest;
        }

        public async Task<ServiceResult> DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return ServiceResult.NotFound;

            _userRepository.Delete(user);
            return await _userRepository.SaveChangesAsync() ? ServiceResult.Success : ServiceResult.BadRequest;
        }
    }
}