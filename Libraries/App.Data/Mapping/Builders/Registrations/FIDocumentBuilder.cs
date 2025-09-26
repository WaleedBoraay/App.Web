using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Registrations;
using App.Core.Builders;
using App.Core.Domain.Users;

namespace App.Data.Mapping.Builders.Registrations;

/// <summary>
/// Represents a FIDocument entity builder
/// </summary>
public partial class FIDocumentBuilder : EntityBuilder<FIDocument>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.WithColumn(nameof(FIDocument.DocumentTypeId)).AsInt32().Nullable();
        table.WithColumn(nameof(FIDocument.UploadedOnUtc)).AsDateTime2().NotNullable();
    }
}