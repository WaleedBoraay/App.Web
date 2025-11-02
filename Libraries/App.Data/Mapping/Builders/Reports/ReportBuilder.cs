using App.Core.Builders;
using App.Core.Domain.Reports;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Reports
{
    public partial class ReportBuilder : EntityBuilder<Report>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Report.Title)).AsString(200).Nullable()
                .WithColumn(nameof(Report.ReportType)).AsString(50).Nullable()
                .WithColumn(nameof(Report.FilePath)).AsString(500).Nullable();
        }
    }
}