using Domain.Entities;

namespace Domain.Repositories;

public interface IFavoritoRepository
{
    Task<IEnumerable<Favorito>> GetByUsuarioAsync(int usuarioId);
    Task<bool> ExistsAsync(int usuarioId, int tmdbMovieId);
    Task AddAsync(int usuarioId, int tmdbMovieId, string titulo, string? posterPath);
    Task RemoveAsync(int usuarioId, int tmdbMovieId);
}
