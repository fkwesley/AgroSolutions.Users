using Application.DTO.Auth;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> GenerateTokenAsync(User user);
    }
}
