using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Institutions;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Institutions;

/// <summary>
/// Represents a Job entity builder
/// </summary>
public partial class JobBuilder : EntityBuilder<Job>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(Job.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(Job.Name)).AsString(256).Nullable()
        .WithColumn(nameof(Job.IsActive)).AsBoolean().Nullable();
}