using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    // #SOLID - Single Responsibility Principle (SRP)
    // UserRepository tem uma única responsabilidade: gerenciar a persistência de usuários.
    // Não contém lógica de negócio, apenas operações de banco de dados.
    
    // #SOLID - Dependency Inversion Principle (DIP)
    // Implementa a interface IUserRepository definida no domínio.
    // A infraestrutura depende do domínio, não o contrário (inversão de dependência).
    
    // #SOLID - Liskov Substitution Principle (LSP)
    // UserRepository pode ser substituído por qualquer outra implementação de IUserRepository
    // (ex: InMemoryUserRepository, MongoUserRepository) sem quebrar o código cliente.
    public class UserRepository : IUserRepository
    {
        private readonly UsersDbContext _context;

        public UserRepository(UsersDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
           return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToUpper() == userId.ToUpper());
        }

        public async Task<User> AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            // Procura por uma instância já rastreada desse usuário
            var trackedEntity = _context.ChangeTracker.Entries<User>().FirstOrDefault(e => e.Entity.UserId == user.UserId);

            // Desanexa a entidade rastreada para evitar conflito
            if (trackedEntity != null)
                trackedEntity.State = EntityState.Detached;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToUpper() == userId.ToUpper());

            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
