using FluentMigrator.Builders.Create.Table;
using App.Core.Builders;
using App.Core.Domain.Audit;
using App.Core.Domain.Users;

namespace App.Data.Mapping.Builders.Audit
{
    /// <summary>
    /// Represents the AuditTrail entity builder
    /// </summary>
    public partial class AuditTrailBuilder : EntityBuilder<AuditTrail>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(AuditTrail.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(AuditTrail.EntityName)).AsString(256).Nullable()
                .WithColumn(nameof(AuditTrail.EntityId)).AsInt32().Nullable()
                .WithColumn(nameof(AuditTrail.FieldName)).AsString(256).Nullable()
                .WithColumn(nameof(AuditTrail.OldValue)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(AuditTrail.NewValue)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(AuditTrail.ChangedOnUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof(AuditTrail.ActionId)).AsInt32().Nullable()
                .WithColumn(nameof(AuditTrail.ClientIp)).AsString(64).Nullable()
                .WithColumn(nameof(AuditTrail.Comment)).AsString(1000).Nullable();
        }
    }
}
