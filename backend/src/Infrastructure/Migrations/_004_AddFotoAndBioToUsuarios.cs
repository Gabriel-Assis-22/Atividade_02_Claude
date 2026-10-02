using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(4, "Adiciona campos de foto_chave, foto_url e bio na tabela usuarios")]
public class _004_AddFotoAndBioToUsuarios : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            ALTER TABLE `usuarios` 
            ADD COLUMN IF NOT EXISTS `foto_chave` VARCHAR(255) NULL,
            ADD COLUMN IF NOT EXISTS `foto_url` VARCHAR(500) NULL,
            ADD COLUMN IF NOT EXISTS `bio` VARCHAR(500) NULL;
        ");
    }

    public override void Down()
    {
        Execute.Sql(@"
            ALTER TABLE `usuarios` 
            DROP COLUMN IF EXISTS `foto_chave`,
            DROP COLUMN IF EXISTS `foto_url`,
            DROP COLUMN IF EXISTS `bio`;
        ");
    }
}
