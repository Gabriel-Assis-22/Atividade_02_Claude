using Dapper;
using Domain.Entities;
using Domain.Repositories;

namespace Infrastructure.Persistence;

public class UsuarioRepository(DbConnectionFactory factory) : IUsuarioRepository
{
    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        using var conn = factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Usuario>(
            "SELECT id AS Id, nome AS Nome, email AS Email, senha_hash AS SenhaHash, criado_em AS CriadoEm FROM usuarios WHERE email = @Email",
            new { Email = email });
    }

    public async Task<int> CreateAsync(string nome, string email, string senhaHash)
    {
        using var conn = factory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO usuarios (nome, email, senha_hash) VALUES (@Nome, @Email, @SenhaHash); SELECT LAST_INSERT_ID();",
            new { Nome = nome, Email = email, SenhaHash = senhaHash });
    }
}
