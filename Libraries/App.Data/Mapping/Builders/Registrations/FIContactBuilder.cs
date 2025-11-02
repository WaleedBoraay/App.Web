using App.Core.Builders;
using App.Core.Domain.Registrations;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Registrations;

/// <summary>
/// Represents a FIContact entity builder
/// </summary>
public partial class FIContactBuilder : EntityBuilder<Contact>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
        .WithColumn(nameof(Contact.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(Contact.RegistrationId)).AsInt32().Nullable()
            .ForeignKey(nameof(Registration), nameof(Registration.Id))

        // Enum-based FK
        .WithColumn(nameof(Contact.ContactTypeId)).AsInt32().Nullable()

        .WithColumn(nameof(Contact.JobTitle)).AsString(256).Nullable()
        .WithColumn(nameof(Contact.FirstName)).AsString(256).Nullable()
        .WithColumn(nameof(Contact.MiddleName)).AsString(256).Nullable()
        .WithColumn(nameof(Contact.LastName)).AsString(256).Nullable()
        .WithColumn(nameof(Contact.ContactPhone)).AsString(50).Nullable()
        .WithColumn(nameof(Contact.BusinessPhone)).AsString(50).Nullable()
        .WithColumn(nameof(Contact.Email)).AsString(256).Nullable()
        .WithColumn(nameof(Contact.NationalityCountryId)).AsInt32().Nullable()
        .WithColumn(nameof(Contact.CreatedOnUtc)).AsDateTime().Nullable()
        .WithColumn(nameof(Contact.UpdatedOnUtc)).AsDateTime().Nullable();
    }
}
