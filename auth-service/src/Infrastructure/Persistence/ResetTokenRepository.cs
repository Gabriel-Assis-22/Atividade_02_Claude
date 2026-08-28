using Dapper;
using Domain.Entities;
using Domain.Repositories;

namespace Infrastructure.Persistence;

public class ResetTokenRepository(DbConnectionFactory factory) : IResetTokenRepository
{
    public async Task CreateAsync(ResetToken token)
    {
        using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            @"INSERT INTO reset_tokens (token, usuario_id, criado_em, expira_em, usado)
              VALUES (@Token, @UsuarioId, @CriadoEm, @ExpiraEm, @Usado)",
            new
            {
                token.Token, token.UsuarioId,
                token.CriadoEm, token.ExpiraEm,
                Usado = false,
            });
    }

    public async Task<ResetToken?> GetByTokenAsync(string token)
    {
        using var conn = factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ResetToken>(
            @"SELECT token AS Token, usuario_id AS UsuarioId,
                     criado_em AS CriadoEm, expira_em AS ExpiraEm, usado AS Usado
              FROM reset_tokens WHERE token = @Token",
            new { Token = token });
    }

    public async Task MarkAsUsedAsync(string token)
    {
        using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE reset_tokens SET usado = TRUE WHERE token = @Token",
            new { Token = token });
    }
}
