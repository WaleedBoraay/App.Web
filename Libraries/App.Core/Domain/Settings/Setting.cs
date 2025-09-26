using App.Core;
using App.Core.Localization;
using App.Core.Domain;

namespace App.Core.Domain.Settings
{
    /// <summary>
    /// Represents a setting
    /// </summary>
    public partial class Setting : BaseEntity, ILocalizedEntity
    {
        #region Ctor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Setting()
        {
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <param name="value">Setting value</param>
        /// <param name="storeId">Store identifier</param>
        public Setting(string name, string value, int storeId = 0)
        {
            Name = name;
            Value = value;
            StoreId = storeId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the setting name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the setting value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the store identifier for which this setting is valid; 0 means all stores
        /// </summary>
        public int StoreId { get; set; }

        #endregion
    }
}