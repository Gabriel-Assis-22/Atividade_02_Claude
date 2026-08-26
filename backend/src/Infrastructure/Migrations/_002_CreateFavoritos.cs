using FluentMigrator;

namespace Infrastructure.Migrations;

[Migration(2, "Cria tabela de favoritos")]
public class _002_CreateFavoritos : Migration
{
    public override void Up()
    {
        Create.Table("favoritos")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("usuario_id").AsInt32().NotNullable()
                .ForeignKey("usuarios", "id")
            .WithColumn("tmdb_movie_id").AsInt32().NotNullable()
            .WithColumn("titulo").AsString(255).NotNullable()
            .WithColumn("poster_path").AsString(255).Nullable()
            .WithColumn("criado_em").AsDateTime().NotNullable()
                .WithDefaultValue(SystemMethods.CurrentDateTime);

        Create.UniqueConstraint("uq_favoritos_usuario_filme")
            .OnTable("favoritos")
            .Columns("usuario_id", "tmdb_movie_id");
    }

    public override void Down() => Delete.Table("favoritos");
}
