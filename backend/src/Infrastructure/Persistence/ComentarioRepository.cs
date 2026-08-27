using Dapper;
using Domain.Entities;
using Domain.Repositories;

namespace Infrastructure.Persistence;

public class ComentarioRepository(DbConnectionFactory factory) : IComentarioRepository
{
    public async Task<IEnumerable<Comentario>> GetByUsuarioAndMovieAsync(int usuarioId, int tmdbMovieId)
    {
        using var conn = factory.CreateConnection();
        return await conn.QueryAsync<Comentario>(
            "SELECT id AS Id, usuario_id AS UsuarioId, tmdb_movie_id AS TmdbMovieId, texto AS Texto, criado_em AS CriadoEm FROM comentarios WHERE tmdb_movie_id = @TmdbMovieId AND usuario_id = @UsuarioId ORDER BY criado_em DESC",
            new { TmdbMovieId = tmdbMovieId, UsuarioId = usuarioId });
    }

    public async Task AddAsync(int usuarioId, int tmdbMovieId, string texto)
    {
        using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "INSERT INTO comentarios (usuario_id, tmdb_movie_id, texto) VALUES (@UsuarioId, @TmdbMovieId, @Texto)",
            new { UsuarioId = usuarioId, TmdbMovieId = tmdbMovieId, Texto = texto });
    }
}
