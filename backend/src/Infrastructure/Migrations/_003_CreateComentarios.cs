using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(3, "Cria tabela de comentarios")]
public class _003_CreateComentarios : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            CREATE TABLE IF NOT EXISTS `comentarios` (
                `id` INT NOT NULL AUTO_INCREMENT,
                `usuario_id` INT NOT NULL,
                `tmdb_movie_id` INT NOT NULL,
                `texto` TEXT NOT NULL,
                `criado_em` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (`id`),
                CONSTRAINT `fk_comentarios_usuario` 
                    FOREIGN KEY (`usuario_id`) 
                    REFERENCES `usuarios` (`id`) 
                    ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
        ");
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS `comentarios`;");
    }
}
