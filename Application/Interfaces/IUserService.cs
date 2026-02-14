using Application.DTO.Common;
using Application.DTO.User;
using Domain.Entities;

namespace Application.Interfaces
{
    // #SOLID - Interface Segregation Principle (ISP)
    // Esta interface define apenas os métodos relacionados a operações de usuários.
    // Clientes que dependem dela não são forçados a depender de métodos que não usam.
    
    // #SOLID - Dependency Inversion Principle (DIP)
    // Esta interface permite que camadas superiores (API) dependam de abstração,
    // não da implementação concreta (UserService).
    public interface IUserService
    {
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
        
        Task<UserResponse> GetUserByIdAsync(string userId);
        Task<UserResponse> AddUserAsync(AddUserRequest user);
        Task<UserResponse> UpdateUserAsync(UpdateUserRequest user);
        Task<bool> DeleteUserAsync(string userId);
        Task<User?> ValidateCredentialsAsync(string userId, string password);
    }
}
