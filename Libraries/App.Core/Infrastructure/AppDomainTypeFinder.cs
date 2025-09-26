using System.Reflection;

namespace App.Core.Infrastructure
{
    /// <summary>
    /// Simple implementation of ITypeFinder that scans loaded assemblies in the current AppDomain.
    /// </summary>
    public class AppDomainTypeFinder : ITypeFinder
    {
        private readonly bool _ignoreReflectionErrors;
        public AppDomainTypeFinder(bool ignoreReflectionErrors = true)
        {
            _ignoreReflectionErrors = ignoreReflectionErrors;
        }

        public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
            => FindClassesOfType(typeof(T), onlyConcreteClasses);

        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true)
        {
            var result = new List<Type>();
            foreach (var assembly in GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException ex)
                {
                    if (!_ignoreReflectionErrors) throw;
                    types = ex.Types.Where(t => t != null)!.ToArray()!;
                }
                catch
                {
                    if (!_ignoreReflectionErrors) throw;
                    continue;
                }

                foreach (var t in types)
                {
                    if (t == null) continue;
                    if (assignTypeFrom.IsAssignableFrom(t) && (!onlyConcreteClasses || (t.IsClass && !t.IsAbstract)))
                        result.Add(t);
                }
            }
            return result;
        }

        public IList<Assembly> GetAssemblies()
            => AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).ToList();

        public Assembly GetAssemblyByName(string assemblyFullName)
            => AppDomain.CurrentDomain.GetAssemblies()
                   .FirstOrDefault(a => a.FullName == assemblyFullName)
               ?? throw new InvalidOperationException($"Assembly not found: {assemblyFullName}");
    }
}
