using WebApi.Entities;
using WebApi.Models.Users;

namespace WebApi.Services
{
    public interface IUserService
    {
        IEnumerable<User> GetAllUsers();
        Task<User> GetUserByIdAsync(int id);
        Task CreateUserAsync(CreateRequest model);
        Task UpdateUserAsync(int id, UpdateRequest model);
        Task DeleteUserAsync(int id);
    }
}
