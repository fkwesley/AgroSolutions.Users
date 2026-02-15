using Application.DTO.User;
using Application.Exceptions;
using Application.Interfaces;
using Application.Mappings;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasherRepository _passwordHasher;
        private readonly ILogger<UserService> _logger;

        // #SOLID - Dependency Inversion Principle (DIP)
        // Constructor Injection: Todas as dependências são abstrações (interfaces).
        // Isso facilita testes unitários (mocks) e desacoplamento da implementação.
        public UserService(
                IUserRepository UserRepository, 
                IPasswordHasherRepository passwordHasher,
                ILogger<UserService> logger)
        {
            _userRepository = UserRepository
                ?? throw new ArgumentNullException(nameof(UserRepository));
            _passwordHasher = passwordHasher
                ?? throw new ArgumentNullException(nameof(passwordHasher));
            _logger = logger;
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();

            _logger.LogInformation("Retrieved {UserCount} Users", users.Count());

            return users.Select(User => User.ToResponse()).ToList();
        }

        public async Task<UserResponse> GetUserByIdAsync(string userId)
        {
            var userFound = await _userRepository.GetUserByIdAsync(userId);

            if (userFound == null)
                throw new ValidationException("User not found.");

            return userFound.ToResponse();
        }

        public async Task<UserResponse> AddUserAsync(AddUserRequest user)
        {
            var activeUsers = (await _userRepository.GetAllUsersAsync()).Where(u => u.IsActive);

            if (activeUsers.Any(u => u.UserId == user.UserId.ToUpper()))
                throw new ValidationException("UserId already exists. Try another one.");

            if (activeUsers.Any(u => u.Email == user.Email.ToLower()))
                throw new ValidationException("E-mail already used by another active user. Try another one.");

            var userEntity = user.ToEntity();
            userEntity.SetPassword(user.Password, _passwordHasher);

            var userAdded = await _userRepository.AddUserAsync(userEntity);
            _logger.LogInformation("User {UserID} Added.", user.UserId);

            return userAdded.ToResponse();
        }

        public async Task<UserResponse> UpdateUserAsync(UpdateUserRequest user)
        {
            var activeUsers = (await _userRepository.GetAllUsersAsync()).Where(u => u.IsActive);

            if (!activeUsers.Any(u => u.UserId == user.UserId.ToUpper()))
                throw new KeyNotFoundException($"User with ID {user.UserId} not found.");

            if (activeUsers.Any(u => u.UserId != user.UserId.ToUpper() && u.Email == user.Email.ToLower()))
                throw new ValidationException("E-mail already used by another active user. Try another one.");

            var userEntity = user.ToEntity();
            userEntity.SetPassword(user.Password, _passwordHasher);
            userEntity.UpdatedAt = DateTime.UtcNow;
            userEntity.Email = user.Email.ToLower();
            userEntity.IsActive = user.IsActive;
            userEntity.IsAdmin = user.IsAdmin;
            userEntity.CreatedAt = activeUsers.FirstOrDefault(u => u.UserId == user.UserId.ToUpper()).CreatedAt;

            var userUpdated = await _userRepository.UpdateUserAsync(userEntity);
            _logger.LogInformation("User {UserId} updated. IsActive: {IsActive}.", userUpdated.UserId, userUpdated.IsActive);

            return userUpdated.ToResponse();
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var deleteStatus = await _userRepository.DeleteUserAsync(userId.ToUpper());
            _logger.LogInformation("User {UserId} deleted.", userId);

            return deleteStatus;
        }

        public async Task<User?> ValidateCredentialsAsync(string userId, string password)
        {
            var userFound = await _userRepository.GetUserByIdAsync(userId.ToUpper());

            if (userFound != null && _passwordHasher.VerifyPassword(password, userFound.PasswordHash))
                return userFound;
            else
                throw new UnauthorizedAccessException("User or password invalid.");
        }

    }
}
