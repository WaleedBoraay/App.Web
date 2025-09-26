using App.Core.Builders;
using App.Core.Domain.Institutions;
using App.Core.Domain.Registrations;
using App.Core.Domain.Security;
using App.Core.Domain.Users;
using FluentMigrator.Builders.Create.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.Mapping.Builders.Instituts
{
    public partial class InstituteDocumentBuilder : EntityBuilder<InstituteDocument>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
            .WithColumn(nameof(InstituteDocument.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(InstituteDocument.InstituteId)).AsInt32().Nullable()
                .ForeignKey(nameof(Institution), nameof(Institution.Id))
            .WithColumn(nameof(InstituteDocument.DocumentId)).AsInt32().Nullable()
                .ForeignKey(nameof(FIDocument), nameof(FIDocument.Id));

        }
    }
}
