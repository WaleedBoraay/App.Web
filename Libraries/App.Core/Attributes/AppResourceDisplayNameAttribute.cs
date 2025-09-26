using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class AppResourceDisplayNameAttribute : Attribute
    {
        public string ResourceKey { get; }

        public AppResourceDisplayNameAttribute(string resourceKey)
        {
            ResourceKey = resourceKey;
        }
    }
}
