using SimpleExample.Application.Interfaces;
using SimpleExample.Domain.Entities;

namespace SimpleExample.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of user repository for testing and demo purposes.
/// No database required - data is stored in memory and initialized with sample data.
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users;
    private readonly object _lock = new object();

    public InMemoryUserRepository()
    {
        _users = new List<User>();
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        DateTime now = DateTime.UtcNow;

        // Käytä konstruktoria käyttäjien luomiseen
        User user1 = new User("Matti", "Meikäläinen", "matti.meikalainen@example.com");
        user1.Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        user1.CreatedAt = now.AddDays(-30);
        user1.UpdatedAt = now.AddDays(-30);

        User user2 = new User("Maija", "Virtanen", "maija.virtanen@example.com");
        user2.Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        user2.CreatedAt = now.AddDays(-15);
        user2.UpdatedAt = now.AddDays(-5);

        User user3 = new User("Teppo", "Testaaja", "teppo.testaaja@example.com");
        user3.Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        user3.CreatedAt = now.AddDays(-7);
        user3.UpdatedAt = now.AddDays(-1);

        _users.AddRange(new[] { user1, user2, user3 });
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        lock (_lock)
        {
            User? user = _users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(user);
        }
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        lock (_lock)
        {
            IEnumerable<User> users = _users.ToList();
            return Task.FromResult(users);
        }
    }

    public Task<User> AddAsync(User entity)
    {
        lock (_lock)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            _users.Add(entity);
            return Task.FromResult(entity);
        }
    }

    public Task<User> UpdateAsync(User entity)
    {
        lock (_lock)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == entity.Id);
            if (existingUser != null)
            {
                existingUser.UpdateBasicInfo(entity.FirstName, entity.LastName);
                existingUser.UpdateEmail(entity.Email);

                existingUser.UpdatedAt = DateTime.UtcNow;
                return Task.FromResult(existingUser);
            }

            return Task.FromResult(entity);
        }
    }

    public Task DeleteAsync(Guid id)
    {
        lock (_lock)
        {
            User? user = _users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _users.Remove(user);
            }
            return Task.CompletedTask;
        }
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        lock (_lock)
        {
            bool exists = _users.Any(u => u.Id == id);
            return Task.FromResult(exists);
        }
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        lock (_lock)
        {
            User? user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }
    }
}
