using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(1, "Cria tabela de usuarios")]
public class _001_CreateUsuarios : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            CREATE TABLE IF NOT EXISTS `usuarios` (
                `id` INT NOT NULL AUTO_INCREMENT,
                `nome` VARCHAR(100) NOT NULL,
                `email` VARCHAR(150) NOT NULL UNIQUE,
                `senha_hash` VARCHAR(255) NOT NULL,
                `criado_em` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (`id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS `usuarios`;");
    }
}
