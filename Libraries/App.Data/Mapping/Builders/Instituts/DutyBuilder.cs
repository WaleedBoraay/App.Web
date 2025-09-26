using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Institutions;
using App.Core.Builders;
using App.Core.Domain.Users;

namespace App.Data.Mapping.Builders.Institutions;

/// <summary>
/// Represents a Duty entity builder
/// </summary>
public partial class DutyBuilder : EntityBuilder<Duty>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(Duty.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(Duty.EmployeeId)).AsInt32().Nullable()
            .ForeignKey(nameof(AppUser), nameof(AppUser.Id))
        .WithColumn(nameof(Duty.DutyType)).AsString(256).Nullable()
        .WithColumn(nameof(Duty.FromDate)).AsDateTime2().Nullable()
        .WithColumn(nameof(Duty.ToDate)).AsDateTime2().Nullable()
        .WithColumn(nameof(Duty.Notes)).AsString(2000).Nullable();
}