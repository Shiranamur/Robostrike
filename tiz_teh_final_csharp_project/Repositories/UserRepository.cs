// File: `tiz_teh_final_csharp_project/Repositories/UserRepository.cs`
using System.Threading.Tasks;
using MySqlConnector;

namespace tiz_teh_final_csharp_project.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<string?> GetUserIdByTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }
            DateTime expiresAt = DateTime.UtcNow;
            const string query = "SELECT UserId FROM Sessions WHERE SessionId = @token and ExpiresAt = @dateTime";
            
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@token", token);
            command.Parameters.AddWithValue("@dateTime", expiresAt);
            
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();

        }
    }
}