using FluentMigrator.Builders.Create.Table;
using App.Core.Builders;
using App.Core.Domain.Correspondences;

namespace App.Data.Mapping.Builders.Correspondences
{
    public partial class CorrespondenceBuilder : EntityBuilder<Correspondence>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Correspondence.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(Correspondence.SenderUserId)).AsInt32().NotNullable()
                .WithColumn(nameof(Correspondence.RecipientUserId)).AsInt32().NotNullable()
                .WithColumn(nameof(Correspondence.Subject)).AsString(256).NotNullable()
                .WithColumn(nameof(Correspondence.Message)).AsString(int.MaxValue).NotNullable()
                .WithColumn(nameof(Correspondence.SentOnUtc)).AsDateTime2().NotNullable()
                .WithColumn(nameof(Correspondence.Status)).AsString(64).NotNullable();
        }
    }
}