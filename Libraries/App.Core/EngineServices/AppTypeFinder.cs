using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace App.Core.EngineServices
{
    public partial class AppTypeFinder : ITypeFinder
    {
        /// <summary>
        /// Finds all classes assignable from T in all loaded assemblies
        /// </summary>
        public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), onlyConcreteClasses);
        }

        /// <summary>
        /// Finds all classes assignable from assignTypeFrom in all loaded assemblies
        /// </summary>
        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true)
        {
            var assemblies = GetAssemblies();
            var result = new List<Type>();

            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray();
                }

                foreach (var type in types)
                {
                    if (assignTypeFrom.IsAssignableFrom(type) &&
                        type != assignTypeFrom &&
                        (!onlyConcreteClasses || (type.IsClass && !type.IsAbstract)))
                    {
                        result.Add(type);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all loaded assemblies in current AppDomain
        /// </summary>
        public IList<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().ToList();
        }

        /// <summary>
        /// Gets assembly by its full name
        /// </summary>
        public Assembly GetAssemblyByName(string assemblyFullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.FullName == assemblyFullName);
        }
    }
}
