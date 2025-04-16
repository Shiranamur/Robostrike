// File: `API/Repositories/IUserRepository.cs`
using System.Threading.Tasks;

namespace api.Repositories
{
    public interface IUserRepository
    {
        Task<string?> GetUserIdByTokenAsync(string token);
    }
}