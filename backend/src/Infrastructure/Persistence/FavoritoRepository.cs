using Dapper;
using Domain.Entities;
using Domain.Repositories;

namespace Infrastructure.Persistence;

public class FavoritoRepository(DbConnectionFactory factory) : IFavoritoRepository
{
    public async Task<IEnumerable<Favorito>> GetByUsuarioAsync(int usuarioId)
    {
        using var conn = factory.CreateConnection();
        return await conn.QueryAsync<Favorito>(
            "SELECT id AS Id, usuario_id AS UsuarioId, tmdb_movie_id AS TmdbMovieId, titulo AS Titulo, poster_path AS PosterPath, criado_em AS CriadoEm FROM favoritos WHERE usuario_id = @UsuarioId ORDER BY criado_em DESC",
            new { UsuarioId = usuarioId });
    }

    public async Task<bool> ExistsAsync(int usuarioId, int tmdbMovieId)
    {
        using var conn = factory.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM favoritos WHERE usuario_id = @UsuarioId AND tmdb_movie_id = @TmdbMovieId",
            new { UsuarioId = usuarioId, TmdbMovieId = tmdbMovieId });
        return count > 0;
    }

    public async Task AddAsync(int usuarioId, int tmdbMovieId, string titulo, string? posterPath)
    {
        using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "INSERT IGNORE INTO favoritos (usuario_id, tmdb_movie_id, titulo, poster_path) VALUES (@UsuarioId, @TmdbMovieId, @Titulo, @PosterPath)",
            new { UsuarioId = usuarioId, TmdbMovieId = tmdbMovieId, Titulo = titulo, PosterPath = posterPath });
    }

    public async Task RemoveAsync(int usuarioId, int tmdbMovieId)
    {
        using var conn = factory.CreateConnection();
        // WHERE duplo — garante isolamento
        await conn.ExecuteAsync(
            "DELETE FROM favoritos WHERE tmdb_movie_id = @TmdbMovieId AND usuario_id = @UsuarioId",
            new { TmdbMovieId = tmdbMovieId, UsuarioId = usuarioId });
    }
}
