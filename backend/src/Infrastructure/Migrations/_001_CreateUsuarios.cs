using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(1, "Cria tabela de usuarios")]
public class _001_CreateUsuarios : Migration
{
    public override void Up()
    {
        if (!Schema.Table("usuarios").Exists())
        {
            Create.Table("usuarios")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("nome").AsString(100).NotNullable()
                .WithColumn("email").AsString(150).NotNullable().Unique()
                .WithColumn("senha_hash").AsString(255).NotNullable()
                .WithColumn("criado_em").AsDateTime().NotNullable()
                    .WithDefaultValue(SystemMethods.CurrentDateTime);
        }
    }

    public override void Down()
    {
        if (Schema.Table("usuarios").Exists())
        {
            Delete.Table("usuarios");
        }
    }
}
