using Domain.Entities;

namespace Domain.Repositories;

public interface IComentarioRepository
{
    Task<IEnumerable<Comentario>> GetByUsuarioAndMovieAsync(int usuarioId, int tmdbMovieId);
    Task AddAsync(int usuarioId, int tmdbMovieId, string texto);
}
