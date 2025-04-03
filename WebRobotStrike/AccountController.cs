using BlazorApp1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorApp1.Controllers
{
    public class AccountController : Controller
    {
        private readonly RobostrikeContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(RobostrikeContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Vérifier si l'utilisateur existe
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return Unauthorized("Utilisateur non trouvé.");
            }

            // Vérifier le mot de passe
            var hashedPassword = User.has(password, user.Salt);
            if (hashedPassword != user.PasswordHash)
            {
                return Unauthorized("Mot de passe incorrect.");
            }

            // Créer un cookie d'authentification
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Login");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("Identity", claimsPrincipal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.  SignOutAsync("Identity");
            return RedirectToAction("Index", "Home");
        }
    }
}
