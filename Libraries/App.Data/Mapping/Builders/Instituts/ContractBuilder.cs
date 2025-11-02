using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Institutions;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Institutions;

/// <summary>
/// Represents a Contract entity builder
/// </summary>
public partial class ContractBuilder : EntityBuilder<Contract>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(Contract.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(Contract.Name)).AsString(256).Nullable()
        .WithColumn(nameof(Contract.Code)).AsString(64).Nullable()
        .WithColumn(nameof(Contract.IsActive)).AsBoolean().Nullable();
}