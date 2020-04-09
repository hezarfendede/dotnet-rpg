using dotnet_rpg.Models;
using System.Threading.Tasks;

namespace dotnet_rpg.Data
{
    public interface IAuthRepository
    {
        Task<ServiceResponse<int>> Register(User user, string password);
        Task<ServiceResponse<string>> Login(string userName, string password);
        Task<bool> UserExists(string userName);
    }
}