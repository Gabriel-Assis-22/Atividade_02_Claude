using FluentMigrator.Runner.VersionTableInfo;

namespace Infrastructure.Migrations;

[VersionTableMetaData]
public class CatalogVersionTableMetaData : IVersionTableMetaData
{
    public object? ApplicationContext { get; set; }
    public string SchemaName => "";
    public string TableName => "VersionInfo_Catalog";
    public string ColumnName => "Version";
    public string UniqueIndexName => "UC_Version_Catalog";
    public string AppliedOnColumnName => "AppliedOn";
    public string DescriptionColumnName => "Description";
    public bool OwnsSchema => true;
    public bool CreateWithNonNullDescription => false;
    public bool CreateWithPrimaryKey => false;
}
