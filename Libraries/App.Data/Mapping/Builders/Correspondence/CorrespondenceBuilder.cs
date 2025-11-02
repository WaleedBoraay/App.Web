using App.Core.Builders;
using App.Core.Domain.Correspondences;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Correspondencess
{
    public partial class CorrespondenceBuilder : EntityBuilder<Correspondence>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Correspondence.SenderUserId)).AsInt32().Nullable()
                .WithColumn(nameof(Correspondence.RecipientUserId)).AsInt32().Nullable()
                .WithColumn(nameof(Correspondence.Subject)).AsString(250).Nullable()
                .WithColumn(nameof(Correspondence.Message)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(Correspondence.SentOnUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof(Correspondence.Status)).AsString(50).Nullable();
        }
    }
}