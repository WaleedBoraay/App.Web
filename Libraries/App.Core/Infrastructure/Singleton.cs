using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Infrastructure
{
    /// <summary>
    /// A statically compiled "singleton" used to store objects throughout the 
    /// lifetime of the App domain. Not so much singleton in the pattern's 
    /// sense of the word as a standardized way to store single instances.
    /// </summary>
    /// <typeparam name="T">The type of object to store.</typeparam>
    /// <remarks>Access to the instance is not synchronized.</remarks>
    public static class Singleton<T> where T : class
    {
        private static T _instance;

        public static T Instance
        {
            get => _instance;
            set => _instance = value ?? throw new ArgumentNullException(nameof(value), "Instance cannot be null.");
        }
    }
}
