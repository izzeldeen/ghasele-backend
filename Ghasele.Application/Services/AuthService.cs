using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Ghasele.Application.DTOs;
using Ghasele.Application.Interfaces;
using Ghasele.Domain.Entities;
using Ghasele.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace Ghasele.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingPhone = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (existingPhone != null)
            {
                throw new Exception("User with this phone number already exists.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.PhoneNumber,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = passwordHash,
                FullName = request.FullName
            };

            await _userRepository.AddAsync(user);

            var token = GenerateJwtToken(user);

            return new AuthResponse(token, user.Id, user.Username, user.Email, user.FullName, user.PhoneNumber);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new Exception("Invalid phone number or password.");
            }

            var token = GenerateJwtToken(user);

            return new AuthResponse(token, user.Id, user.Username, user.Email, user.FullName, user.PhoneNumber);
        }

        public async Task UpdateFcmTokenAsync(Guid userId, string token)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.FcmToken = token;
                await _userRepository.UpdateAsync(user);
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is missing");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("username", user.Username)
            };

            if (!string.IsNullOrEmpty(user.Email))
            {
                var claimList = claims.ToList();
                claimList.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
                claims = claimList.ToArray();
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
