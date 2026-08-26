using Application.DTOs.Favorites;
using Domain.Repositories;

namespace Application.UseCases.Favorites;

public class GetFavoritesUseCase(IFavoritoRepository repo)
{
    public async Task<IEnumerable<FavoriteDto>> ExecuteAsync(int usuarioId)
    {
        var favs = await repo.GetByUsuarioAsync(usuarioId);
        return favs.Select(f => new FavoriteDto(f.Id, f.TmdbMovieId, f.Titulo, f.PosterPath, f.CriadoEm));
    }
}

public class AddFavoriteUseCase(IFavoritoRepository repo)
{
    public Task ExecuteAsync(int usuarioId, AddFavoriteRequest req) =>
        repo.AddAsync(usuarioId, req.TmdbMovieId, req.Titulo, req.PosterPath);
}

public class RemoveFavoriteUseCase(IFavoritoRepository repo)
{
    public Task ExecuteAsync(int usuarioId, int tmdbMovieId) =>
        repo.RemoveAsync(usuarioId, tmdbMovieId);
}

public class CheckFavoriteUseCase(IFavoritoRepository repo)
{
    public Task<bool> ExecuteAsync(int usuarioId, int tmdbMovieId) =>
        repo.ExistsAsync(usuarioId, tmdbMovieId);
}
