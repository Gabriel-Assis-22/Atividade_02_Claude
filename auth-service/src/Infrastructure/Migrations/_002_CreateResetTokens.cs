using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(2, "Cria tabela reset_tokens")]
public class _002_CreateResetTokens : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            CREATE TABLE IF NOT EXISTS `reset_tokens` (
                `token` VARCHAR(64) NOT NULL,
                `usuario_id` INT NOT NULL,
                `criado_em` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                `expira_em` DATETIME NOT NULL,
                `usado` BOOLEAN NOT NULL DEFAULT FALSE,
                PRIMARY KEY (`token`),
                CONSTRAINT `fk_reset_tokens_usuario` 
                    FOREIGN KEY (`usuario_id`) 
                    REFERENCES `usuarios` (`id`) 
                    ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS `reset_tokens`;");
    }
}
