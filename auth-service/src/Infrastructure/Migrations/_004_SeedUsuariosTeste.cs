using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(4, "Insere usuários de teste (admin e comuns) para validação do RBAC")]
public class _004_SeedUsuariosTeste : Migration
{
    public override void Up()
    {
        var adminHash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 10);
        var userHash = BCrypt.Net.BCrypt.HashPassword("user123", workFactor: 10);

        Execute.WithConnection((conn, tran) =>
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tran;
            cmd.CommandText = @"
                INSERT INTO `usuarios` (`nome`, `email`, `senha_hash`, `role`)
                SELECT 'Administrador', 'admin@catalogo.com', @AdminHash, 'admin'
                WHERE NOT EXISTS (SELECT 1 FROM `usuarios` WHERE `email` = 'admin@catalogo.com');

                INSERT INTO `usuarios` (`nome`, `email`, `senha_hash`, `role`)
                SELECT 'Usuário 1', 'usuario1@catalogo.com', @UserHash, 'usuario'
                WHERE NOT EXISTS (SELECT 1 FROM `usuarios` WHERE `email` = 'usuario1@catalogo.com');

                INSERT INTO `usuarios` (`nome`, `email`, `senha_hash`, `role`)
                SELECT 'Usuário 2', 'usuario2@catalogo.com', @UserHash, 'usuario'
                WHERE NOT EXISTS (SELECT 1 FROM `usuarios` WHERE `email` = 'usuario2@catalogo.com');
            ";

            var pAdmin = cmd.CreateParameter();
            pAdmin.ParameterName = "@AdminHash";
            pAdmin.Value = adminHash;
            cmd.Parameters.Add(pAdmin);

            var pUser = cmd.CreateParameter();
            pUser.ParameterName = "@UserHash";
            pUser.Value = userHash;
            cmd.Parameters.Add(pUser);

            cmd.ExecuteNonQuery();
        });
    }

    public override void Down()
    {
        Execute.Sql("DELETE FROM `usuarios` WHERE `email` IN ('admin@catalogo.com', 'usuario1@catalogo.com', 'usuario2@catalogo.com');");
    }
}
