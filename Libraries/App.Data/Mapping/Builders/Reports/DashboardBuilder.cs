using App.Core.Builders;
using App.Core.Domain.Reports;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Reports
{
    public partial class DashboardBuilder : EntityBuilder<Dashboard>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Dashboard.Name)).AsString(200).Nullable()
                .WithColumn(nameof(Dashboard.Description)).AsString(500).Nullable();
        }
    }
}