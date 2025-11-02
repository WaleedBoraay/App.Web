using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Common;
using App.Core.Builders;
using App.Core.Domain.Users;

namespace App.Data.Mapping.Builders.Common;

/// <summary>
/// Represents a Attachment entity builder
/// </summary>
public partial class AttachmentBuilder : EntityBuilder<Attachment>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(Attachment.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(Attachment.FileName)).AsString(256).Nullable()
        .WithColumn(nameof(Attachment.StoragePath)).AsString(1024).Nullable()
        .WithColumn(nameof(Attachment.MimeType)).AsString(128).Nullable()
        .WithColumn(nameof(Attachment.Extension)).AsString(20).Nullable()
        .WithColumn(nameof(Attachment.FileSize)).AsInt64().Nullable()
        .WithColumn(nameof(Attachment.UploadedByUserId)).AsInt32().Nullable()
            .ForeignKey(nameof(AppUser), nameof(AppUser.Id))
        .WithColumn(nameof(Attachment.UploadedOnUtc)).AsDateTime2().Nullable();
}