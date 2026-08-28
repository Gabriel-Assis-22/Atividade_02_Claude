using Domain.Entities;

namespace Domain.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<Usuario?> GetByIdAsync(int id);
    Task<int> CreateAsync(string nome, string email, string senhaHash, Role role = Role.usuario);
    Task UpdatePasswordAsync(int usuarioId, string novaSenhaHash);
}
