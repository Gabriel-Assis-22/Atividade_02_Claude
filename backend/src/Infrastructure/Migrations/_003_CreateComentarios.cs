using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(3, "Cria tabela de comentarios")]
public class _003_CreateComentarios : Migration
{
    public override void Up()
    {
        if (!Schema.Table("comentarios").Exists())
        {
            Create.Table("comentarios")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("usuario_id").AsInt32().NotNullable()
                    .ForeignKey("usuarios", "id")
                .WithColumn("tmdb_movie_id").AsInt32().NotNullable()
                .WithColumn("texto").AsString(int.MaxValue).NotNullable()
                .WithColumn("criado_em").AsDateTime().NotNullable()
                    .WithDefaultValue(SystemMethods.CurrentDateTime);
        }
    }

    public override void Down()
    {
        if (Schema.Table("comentarios").Exists())
        {
            Delete.Table("comentarios");
        }
    }
}
