using App.Core.Builders;
using App.Core.Domain.Registrations;
using FluentMigrator.Builders.Create.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.Mapping.Builders.Registrations
{
    public partial class BranchBuilder : EntityBuilder<Branch>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Branch.Id)).AsInt32().PrimaryKey().Identity()

                .WithColumn(nameof(Branch.InstitutionId)).AsInt32().Nullable()
                .WithColumn(nameof(Branch.Name)).AsString(200).Nullable()
                .WithColumn(nameof(Branch.Address)).AsString(500).Nullable()
                .WithColumn(nameof(Branch.Phone)).AsString(50).Nullable()
                .WithColumn(nameof(Branch.Email)).AsString(200).Nullable()

                .WithColumn(nameof(Branch.CountryId)).AsInt32().Nullable()

                .WithColumn(nameof(Branch.CreatedOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(Branch.UpdatedOnUtc)).AsDateTime().Nullable();
        }
    }
}
