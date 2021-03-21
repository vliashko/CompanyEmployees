using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly UserManager<User> userManager; 
        private readonly IConfiguration configuration;
        private User user;

        public AuthenticationManager(UserManager<User> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }
        public async Task<string> CreateToken()
        {
            var signingCredentials = GetSigningCredentials(); 
            var claims = await GetClaims(); 
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims); 
            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        public async Task<bool> ValidateUser(UserForAuthenticationDto userForAuth)
        {
            user = await userManager.FindByNameAsync(userForAuth.UserName); 
            return (user != null && await userManager.CheckPasswordAsync(user, userForAuth.Password));
        }
        private SigningCredentials GetSigningCredentials() 
        { 
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET")); 
            var secret = new SymmetricSecurityKey(key); 
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256); 
        }
        private async Task<List<Claim>> GetClaims() 
        { 
            var claims = new List<Claim>
            { 
                new Claim(ClaimTypes.Name, user.UserName) 
            }; 
            var roles = await userManager.GetRolesAsync(user); 
            foreach (var role in roles) 
            { 
                claims.Add(new Claim(ClaimTypes.Role, role)); 
            } 
            return claims; 
        }
        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims) 
        { 
            var jwtSettings = configuration.GetSection("JwtSettings"); 
            var tokenOptions = new JwtSecurityToken
            (
                issuer: jwtSettings.GetSection("validIssuer").Value, 
                audience: jwtSettings.GetSection("validAudience").Value, 
                claims: claims, 
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("expires").Value)), 
                signingCredentials: signingCredentials
             ); 
            return tokenOptions; 
        }
    }
}
