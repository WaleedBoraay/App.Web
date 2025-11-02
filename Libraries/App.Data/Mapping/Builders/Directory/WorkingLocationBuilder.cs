using App.Core.Builders;
using App.Core.Domain.Directory;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Directory
{
    public partial class WorkingLocationBuilder : EntityBuilder<WorkingLocation>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(WorkingLocation.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(WorkingLocation.Name)).AsString(200).Nullable()
                .WithColumn(nameof(WorkingLocation.Address)).AsString(500).Nullable()
                .WithColumn(nameof(WorkingLocation.CountryId)).AsInt32().Nullable();
        }
    }
}