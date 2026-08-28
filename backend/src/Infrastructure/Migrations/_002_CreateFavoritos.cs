using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(2, "Cria tabela de favoritos")]
public class _002_CreateFavoritos : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            CREATE TABLE IF NOT EXISTS `favoritos` (
                `id` INT NOT NULL AUTO_INCREMENT,
                `usuario_id` INT NOT NULL,
                `tmdb_movie_id` INT NOT NULL,
                `titulo` VARCHAR(255) NOT NULL,
                `poster_path` VARCHAR(255) NULL,
                `criado_em` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (`id`),
                CONSTRAINT `fk_favoritos_usuario` 
                    FOREIGN KEY (`usuario_id`) 
                    REFERENCES `usuarios` (`id`) 
                    ON DELETE CASCADE,
                CONSTRAINT `uq_favoritos_usuario_filme` 
                    UNIQUE (`usuario_id`, `tmdb_movie_id`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS `favoritos`;");
    }
}
