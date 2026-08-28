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
            @"SELECT id AS Id, nome AS Nome, email AS Email,
                     senha_hash AS SenhaHash, role AS Role, criado_em AS CriadoEm
              FROM usuarios WHERE email = @Email",
            new { Email = email });
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        using var conn = factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Usuario>(
            @"SELECT id AS Id, nome AS Nome, email AS Email,
                     senha_hash AS SenhaHash, role AS Role, criado_em AS CriadoEm
              FROM usuarios WHERE id = @Id",
            new { Id = id });
    }

    public async Task<int> CreateAsync(string nome, string email, string senhaHash, Role role = Role.usuario)
    {
        using var conn = factory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO usuarios (nome, email, senha_hash, role)
              VALUES (@Nome, @Email, @SenhaHash, @Role);
              SELECT LAST_INSERT_ID();",
            new { Nome = nome, Email = email, SenhaHash = senhaHash, Role = role.ToString() });
    }

    public async Task UpdatePasswordAsync(int usuarioId, string novaSenhaHash)
    {
        using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE usuarios SET senha_hash = @SenhaHash WHERE id = @Id",
            new { SenhaHash = novaSenhaHash, Id = usuarioId });
    }
}
