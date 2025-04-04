using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using BlazorApp1.Models;
using System.Security.Cryptography;
using System.Text;

namespace BlazorApp1.UsersServices
{
    public class UsersService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RobostrikeContext _context;

        public UsersService(IHttpContextAccessor httpContextAccessor, RobostrikeContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        //Pas utiliser comme cookie marche pas 
        public async Task<User?> GetCurrentUser()
        {
            var sessionId = _httpContextAccessor.HttpContext?.Request.Cookies["SessionID"];
            if (string.IsNullOrEmpty(sessionId))
            {
                return null;
            }
            return await GetUserById(int.Parse(sessionId));
        }

        public async Task<User?> GetUserById(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> TestUsername(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task New_User(string username, string email, bool isEmailValidated, string passwordHash, string salt, int points)
        {
            var newUser = new User
            {
                Username = username,
                Email = email,
                IsEmailValidated = isEmailValidated,
                PasswordHash = passwordHash,
                Salt = salt,
                Points = points
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ValidateUserLogin(string username, string password)
        {
            var user = await GetUserByUsername(username);

            if (user == null)
            {
                return false;
            }

            var passwordHash = Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes(password + user.Salt))
            );

            return user.PasswordHash == passwordHash;
        }

        public void SetUserSession(int userId)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                Console.WriteLine("HttpContext is null");
                return;
            }

            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            context.Response.Cookies.Append("SessionID", userId.ToString(), options);
        }

        public bool IsUserAuthenticated()
        {
            var sessionId = _httpContextAccessor.HttpContext?.Request.Cookies["SessionID"];
            return !string.IsNullOrEmpty(sessionId);
        }

        public bool IsCookieSet()
        {
            var cookieValue = _httpContextAccessor.HttpContext?.Request.Cookies["SessionID"];
            return !string.IsNullOrEmpty(cookieValue);
        }
    }
}