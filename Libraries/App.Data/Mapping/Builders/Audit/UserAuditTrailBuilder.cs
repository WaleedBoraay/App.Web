using App.Core.Builders;
using App.Core.Domain.Audit;
using App.Core.Domain.Users;
using FluentMigrator.Builders.Create.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.Mapping.Builders.Audit
{
    public partial class UserAuditTrailBuilder : EntityBuilder<UserAuditTrail>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(UserAuditTrail.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(UserAuditTrail.AuditTrailId)).AsInt32().Nullable()
                    .ForeignKey(nameof(AuditTrail), nameof(AuditTrail.Id))
                    .WithColumn(nameof(UserAuditTrail.UserId)).AsInt32().Nullable()
                    .ForeignKey(nameof(AppUser), nameof(AppUser.Id));

        }
    }
}
