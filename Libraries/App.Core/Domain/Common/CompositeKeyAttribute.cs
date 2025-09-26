using System;

namespace App.Core.Domain.Common
{
    /// <summary>
    /// Define composite primary key for a mapping table
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public partial class CompositeKeyAttribute : Attribute
    {
        public string[] PropertyNames { get; }

        public CompositeKeyAttribute(params string[] propertyNames)
        {
            PropertyNames = propertyNames;
        }
    }
}
