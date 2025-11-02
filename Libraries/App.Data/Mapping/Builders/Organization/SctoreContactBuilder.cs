using App.Core.Builders;
using App.Core.Domain.Organization;
using App.Core.Domain.Registrations;
using FluentMigrator.Builders.Create.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.Mapping.Builders.Organization
{
    public partial class SctoreContactBuilder : EntityBuilder<SctoreContact>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SctoreContact.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(SctoreContact.SctoreId)).AsInt32().Nullable()
                .ForeignKey(nameof(Sector), nameof(Sector.Id))
                .WithColumn(nameof(SctoreContact.ContactId)).AsInt32().Nullable()
                .ForeignKey(nameof(Contact), nameof(Contact.Id));
		}
    }
}
