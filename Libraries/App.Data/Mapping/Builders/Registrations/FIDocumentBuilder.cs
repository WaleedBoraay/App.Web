using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Registrations;
using App.Core.Builders;
using App.Core.Domain.Users;

namespace App.Data.Mapping.Builders.Registrations;

/// <summary>
/// Represents a FIDocument entity builder
/// </summary>
public partial class FIDocumentBuilder : EntityBuilder<Document>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.WithColumn(nameof(Document.DocumentTypeId)).AsInt32().Nullable();
        table.WithColumn(nameof(Document.UploadedOnUtc)).AsDateTime2().Nullable();
        table.WithColumn(nameof(Document.ContactId)).AsInt32().Nullable();
	}
}