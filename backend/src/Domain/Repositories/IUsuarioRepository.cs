using Domain.Entities;

namespace Domain.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(int id);
    Task UpdateProfileAsync(int id, string? bio);
    Task UpdateFotoAsync(int id, string fotoChave, string fotoUrl);
}
