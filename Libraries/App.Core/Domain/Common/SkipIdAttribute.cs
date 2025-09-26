using System;

namespace App.Core.Domain.Common
{
    /// <summary>
    /// Use this to tell the auto-schema generator to skip creating the default Id column
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public partial class SkipIdAttribute : Attribute
    {
    }
}
