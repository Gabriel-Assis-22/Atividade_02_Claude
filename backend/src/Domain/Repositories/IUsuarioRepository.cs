using Domain.Entities;

namespace Domain.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<int> CreateAsync(string nome, string email, string senhaHash);
}
