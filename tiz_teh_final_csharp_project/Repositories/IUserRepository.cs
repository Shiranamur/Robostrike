// File: `tiz_teh_final_csharp_project/Repositories/IUserRepository.cs`
using System.Threading.Tasks;

namespace tiz_teh_final_csharp_project.Repositories
{
    public interface IUserRepository
    {
        Task<string?> GetUserIdByTokenAsync(string token);
    }
}