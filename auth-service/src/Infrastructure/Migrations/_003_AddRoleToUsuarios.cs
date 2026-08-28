using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(3, "Garante que a coluna role exista na tabela usuarios")]
public class _003_AddRoleToUsuarios : Migration
{
    public override void Up()
    {
        Execute.Sql("ALTER TABLE `usuarios` ADD COLUMN IF NOT EXISTS `role` VARCHAR(20) NOT NULL DEFAULT 'usuario';");
    }

    public override void Down()
    {
    }
}
