using Dapper;
using Domain.Entities;
using Domain.Repositories;

namespace Infrastructure.Persistence;

public class ComentarioRepository(DbConnectionFactory factory) : IComentarioRepository
{
    public async Task<IEnumerable<Comentario>> GetByMovieAsync(int tmdbMovieId)
    {
        using var conn = factory.CreateConnection();
        return await conn.QueryAsync<Comentario>(
            @"SELECT 
                c.id AS Id, 
                c.usuario_id AS UsuarioId, 
                COALESCE(u.nome, 'Usuário') AS UsuarioNome, 
                c.tmdb_movie_id AS TmdbMovieId, 
                c.texto AS Texto, 
                c.criado_em AS CriadoEm 
              FROM comentarios c 
              LEFT JOIN usuarios u ON c.usuario_id = u.id 
              WHERE c.tmdb_movie_id = @TmdbMovieId 
              ORDER BY c.criado_em DESC",
            new { TmdbMovieId = tmdbMovieId });
    }

    public async Task<Comentario?> GetByIdAsync(int id)
    {
        using var conn = factory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Comentario>(
            @"SELECT 
                c.id AS Id, 
                c.usuario_id AS UsuarioId, 
                COALESCE(u.nome, 'Usuário') AS UsuarioNome, 
                c.tmdb_movie_id AS TmdbMovieId, 
                c.texto AS Texto, 
                c.criado_em AS CriadoEm 
              FROM comentarios c 
              LEFT JOIN usuarios u ON c.usuario_id = u.id 
              WHERE c.id = @Id",
            new { Id = id });
    }

    public async Task AddAsync(int usuarioId, int tmdbMovieId, string texto)
    {
        using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "INSERT INTO comentarios (usuario_id, tmdb_movie_id, texto) VALUES (@UsuarioId, @TmdbMovieId, @Texto)",
            new { UsuarioId = usuarioId, TmdbMovieId = tmdbMovieId, Texto = texto });
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM comentarios WHERE id = @Id",
            new { Id = id });
    }
}
