using Domain.Entities;

namespace Domain.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(string userId);
        Task<User> AddUserAsync(User User);
        Task<User> UpdateUserAsync(User User);
        Task<bool> DeleteUserAsync(string userId);
    }
}