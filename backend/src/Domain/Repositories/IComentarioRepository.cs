using Domain.Entities;

namespace Domain.Repositories;

public interface IComentarioRepository
{
    Task<IEnumerable<Comentario>> GetByMovieAsync(int tmdbMovieId);
    Task<Comentario?> GetByIdAsync(int id);
    Task AddAsync(int usuarioId, int tmdbMovieId, string texto);
    Task DeleteAsync(int id);
}
