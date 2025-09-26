
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace App.Data.MappingServices;

public static class NameCompatibilityManager
{
    // thread-safety
    private static readonly object _locker = new();
    private static bool _initialized;

    private static readonly Dictionary<Type, string> _tableNames = new();
    private static readonly Dictionary<(Type entityType, string propertyName), string> _columnNames = new();
    private static readonly HashSet<Type> _loadedFor = new();

    // Optional: you can register extra compatibilities manually here
    public static Type[] AdditionalNameCompatibilities { get; set; } = Array.Empty<Type>();

    private static void Initialize()
    {
        lock (_locker)
        {
            if (_initialized) return;

            // 1) discover any INameCompatibility implementations via reflection (no Engine/DI)
            var compatibilities = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => SafeGetTypes(a))
                .Where(t => typeof(INameCompatibility).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(t => Activator.CreateInstance(t) as INameCompatibility)
                .Where(x => x != null)!
                .Cast<INameCompatibility>()
                .ToList();

            // include any manually provided compatibilities
            compatibilities.AddRange(AdditionalNameCompatibilities
                .Select(t => Activator.CreateInstance(t) as INameCompatibility)
                .Where(x => x != null)!
                .Cast<INameCompatibility>());

            foreach (INameCompatibility nameCompatibility in compatibilities.DistinctBy(c => c!.GetType()))
            {
                if (nameCompatibility is null)
                    continue;

                if (_loadedFor.Contains(nameCompatibility.GetType()))
                    continue;

                _loadedFor.Add(nameCompatibility.GetType());

                if (nameCompatibility.TableNames != null)
                    foreach (var (key, value) in nameCompatibility.TableNames)
                        if (!_tableNames.ContainsKey(key) && !string.IsNullOrWhiteSpace(value))
                            _tableNames.Add(key, value);

                if (nameCompatibility.ColumnName != null)
                    foreach (var (key, value) in nameCompatibility.ColumnName)
                        if (!_columnNames.ContainsKey(key) && !string.IsNullOrWhiteSpace(value))
                            _columnNames.Add(key, value);
            }

            _initialized = true;
        }
    }

    /// <summary>Gets table name for mapping with the type.</summary>
    public static string GetTableName(Type type)
    {
        if (!_initialized) Initialize();

        // 0) [Table] attribute on the entity type
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        if (!string.IsNullOrWhiteSpace(tableAttr?.Name))
            return tableAttr!.Name!;

        // 1) compatibility dictionary
        if (_tableNames.TryGetValue(type, out var value) && !string.IsNullOrWhiteSpace(value))
            return value;

        // 2) fallback: class name (customize pluralization if you want)
        return type.Name;
    }

    /// <summary>Gets column name for mapping with the entity's property and type.</summary>
    public static string GetColumnName(Type entityType, string propertyName)
    {
        if (!_initialized) Initialize();

        // 0) [Column] attribute on the property
        var prop = entityType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        var colAttr = prop?.GetCustomAttribute<ColumnAttribute>();
        if (!string.IsNullOrWhiteSpace(colAttr?.Name))
            return colAttr!.Name!;

        // 1) compatibility dictionary
        if (_columnNames.TryGetValue((entityType, propertyName), out var value) && !string.IsNullOrWhiteSpace(value))
            return value;

        // 2) fallback: property name
        return prop?.Name ?? propertyName;
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly a)
    {
        try { return a.GetTypes(); }
        catch { return Array.Empty<Type>(); }
    }
}
