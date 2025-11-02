using App.Core.Builders;
using FluentMigrator.Builders.Create.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Registrations
{
    public partial class RegistrationDocumentBuilder : EntityBuilder<RegistrationDocument>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(RegistrationDocument.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(RegistrationDocument.RegistrationId)).AsInt32().Nullable()
                    .ForeignKey(nameof(Registration), nameof(Registration.Id))
                .WithColumn(nameof(RegistrationDocument.DocumentId)).AsInt32().Nullable()
                    .ForeignKey(nameof(Document), nameof(Document.Id));
        }
    }
}
