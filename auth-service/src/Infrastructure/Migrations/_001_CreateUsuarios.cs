using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(1, "Cria tabela usuarios com coluna role")]
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
                .WithColumn("role").AsString(20).NotNullable().WithDefaultValue("usuario")
                .WithColumn("criado_em").AsDateTime().NotNullable()
                    .WithDefaultValue(SystemMethods.CurrentDateTime);
        }
        else if (!Schema.Table("usuarios").Column("role").Exists())
        {
            Alter.Table("usuarios")
                .AddColumn("role").AsString(20).NotNullable().WithDefaultValue("usuario");
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
