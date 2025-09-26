using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Security;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Security;

/// <summary>
/// Represents a Privilege entity builder
/// </summary>
public partial class PrivilegeBuilder : EntityBuilder<Privilege>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(Privilege.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(Privilege.SystemName)).AsString(256).Nullable()
        .WithColumn(nameof(Privilege.DisplayName)).AsString(256).Nullable()
        .WithColumn(nameof(Privilege.Description)).AsString(256).Nullable();
}