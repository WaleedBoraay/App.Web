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
                .WithColumn(nameof(Correspondence.SenderUserId)).AsInt32().NotNullable()
                .WithColumn(nameof(Correspondence.RecipientUserId)).AsInt32().NotNullable()
                .WithColumn(nameof(Correspondence.Subject)).AsString(250).NotNullable()
                .WithColumn(nameof(Correspondence.Message)).AsString(int.MaxValue).NotNullable()
                .WithColumn(nameof(Correspondence.SentOnUtc)).AsDateTime2().NotNullable()
                .WithColumn(nameof(Correspondence.Status)).AsString(50).NotNullable();
        }
    }
}