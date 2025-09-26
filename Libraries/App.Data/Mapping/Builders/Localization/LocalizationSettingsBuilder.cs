using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Localization;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Localization;

/// <summary>
/// Represents a LocalizationSettings entity builder
/// </summary>
public partial class LocalizationSettingsBuilder : EntityBuilder<LocalizationSettings>
{
    #region Methods

    /// <summary>
    /// Apply entity configuration
    /// </summary>
    /// <param name="table">Create table expression builder</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
    }

    #endregion
}
