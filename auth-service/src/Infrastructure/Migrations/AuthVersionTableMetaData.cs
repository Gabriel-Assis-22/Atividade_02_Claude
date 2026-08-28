using FluentMigrator.Runner.VersionTableInfo;

namespace Infrastructure.Migrations;

[VersionTableMetaData]
public class AuthVersionTableMetaData : IVersionTableMetaData
{
    public object? ApplicationContext { get; set; }
    public string SchemaName => "";
    public string TableName => "VersionInfo_Auth";
    public string ColumnName => "Version";
    public string UniqueIndexName => "UC_Version_Auth";
    public string AppliedOnColumnName => "AppliedOn";
    public string DescriptionColumnName => "Description";
    public bool OwnsSchema => true;
    public bool CreateWithNonNullDescription => false;
    public bool CreateWithPrimaryKey => false;
}
