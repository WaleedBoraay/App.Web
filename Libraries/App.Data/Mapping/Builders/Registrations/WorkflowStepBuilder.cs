using FluentMigrator.Builders.Create.Table;
using App.Core.Builders;
using App.Core.Domain.Registrations;

namespace App.Data.Mapping.Builders.Registrations
{
    /// <summary>
    /// Represents the WorkflowStep entity builder
    /// </summary>
    public partial class WorkflowStepBuilder : EntityBuilder<WorkflowStep>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(WorkflowStep.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(WorkflowStep.Name)).AsString(256).NotNullable()
                .WithColumn(nameof(WorkflowStep.NextStepId)).AsInt32().Nullable()
                .WithColumn(nameof(WorkflowStep.RoleAllowedId)).AsInt32().NotNullable();
        }
    }
}
