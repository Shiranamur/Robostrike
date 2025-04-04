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

        public async Task<User?> ValidateSessionTokenAsync(string token)
        {
            // Query the Sessions table
            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s =>
                    s.SessionId == token &&
                    s.IsActive &&
                    s.ExpiresAt > DateTime.UtcNow
                );

            // If session is invalid/not found/expired, return null
            if (session == null)
                return null;

            // Optional: For "sliding" expiration, update ExpiresAt here
            // session.ExpiresAt = DateTime.UtcNow.AddHours(1);
            // await _context.SaveChangesAsync();

            // Return the associated user
            return session.User;
        }

        public async Task<bool> InvalidateSessionTokenAsync(string token)
        {
            // Mark the session inactive (logout scenario)
            var session = await _context.Sessions.FindAsync(token);
            if (session == null) return false;

            session.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
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
