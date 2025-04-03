using System;
using System.Collections.Generic;

namespace BlazorApp1.Models
{
    public partial class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public bool IsEmailValidated { get; set; }

        public string PasswordHash { get; set; } = null!;

        public string Salt { get; set; } = null!;

        public int Points { get; set; }

        // Exemple de méthode pour hacher le mot de passe
        public static string HashPassword(string password, string salt)
        {
            var saltedPassword = password + salt;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
