using BlazorApp1.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlazorApp1.Services
{
    public class SessionService
    {
        private readonly RobostrikeContext _context;

        public SessionService(RobostrikeContext context)
        {
            _context = context;
        }

        public async Task<string> CreateSessionTokenAsync(int userId)
        {
            // Generate random token
            var token = GenerateRandomToken();

            // Create and save new session
            var session = new Session
            {
                SessionId = token,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(4), // or however long
                IsActive = true
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return token;
        }
        
        private string GenerateRandomToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
