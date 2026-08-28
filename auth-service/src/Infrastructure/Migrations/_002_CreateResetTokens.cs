using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(2, "Cria tabela reset_tokens")]
public class _002_CreateResetTokens : Migration
{
    public override void Up()
    {
        Create.Table("reset_tokens")
            .WithColumn("token").AsString(64).PrimaryKey()
            .WithColumn("usuario_id").AsInt32().NotNullable()
                .ForeignKey("usuarios", "id")
            .WithColumn("criado_em").AsDateTime().NotNullable()
                .WithDefaultValue(SystemMethods.CurrentDateTime)
            .WithColumn("expira_em").AsDateTime().NotNullable()
            .WithColumn("usado").AsBoolean().NotNullable().WithDefaultValue(false);
    }

    public override void Down() => Delete.Table("reset_tokens");
}
