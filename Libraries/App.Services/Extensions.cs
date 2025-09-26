using Microsoft.AspNetCore.Mvc.Rendering;
using App.Core;
using App.Core.Infrastructure;
using App.Core.Domain;
using App.Core.EngineServices;
using App.Services.Localization;
using App.Data.EngineServices;

namespace App.Services;

/// <summary>
/// Extensions
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Convert to select list
    /// </summary>
    /// <typeparam name="TEnum">Enum type</typeparam>
    /// <param name="enumObj">Enum</param>
    /// <param name="markCurrentAsSelected">Mark current value as selected</param>
    /// <param name="valuesToExclude">Values to exclude</param>
    /// <param name="useLocalization">Localize</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the selectList
    /// </returns>
    public static async Task<SelectList> ToSelectListAsync<TEnum>(this TEnum enumObj,
        bool markCurrentAsSelected = true, int[] valuesToExclude = null, bool useLocalization = true) where TEnum : Enum
    {
        if (enumObj == null)
            throw new ArgumentNullException(nameof(enumObj));

        var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

        var values = await Task.WhenAll(
            Enum.GetValues(typeof(TEnum)).OfType<TEnum>()
                .Where(enumValue => valuesToExclude == null || !valuesToExclude.Contains(Convert.ToInt32(enumValue)))
                .Select(async enumValue => new
                {
                    ID = Convert.ToInt32(enumValue),
                    Name = useLocalization
                        ? await localizationService.GetResourceAsync($"Enums.{typeof(TEnum).Name}.{enumValue}")
                        : CommonHelper.SplitCamelCaseWord(enumValue.ToString())
                })
        );

        object selectedValue = null;
        if (markCurrentAsSelected)
            selectedValue = Convert.ToInt32(enumObj);

        return new SelectList(values, "ID", "Name", selectedValue);
    }

    /// <summary>
    /// Convert to select list
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="objList">List of objects</param>
    /// <param name="selector">Selector for name</param>
    /// <returns>SelectList</returns>
    public static SelectList ToSelectList<T>(this T objList, Func<BaseEntity, string> selector) where T : IEnumerable<BaseEntity>
    {
        return new SelectList(objList.Select(p => new { ID = p.Id, Name = selector(p) }), "ID", "Name");
    }

    /// <summary>
    /// Convert to lookup-like dictionary, for JSON serialization
    /// </summary>
    /// <typeparam name="T">Source type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    /// <param name="xs">List of objects</param>
    /// <param name="keySelector">A key-selector function</param>
    /// <param name="valueSelector">A value-selector function</param>
    /// <returns>A dictionary with values grouped by key</returns>
    public static IDictionary<TKey, IList<TValue>> ToGroupedDictionary<T, TKey, TValue>(
        this IEnumerable<T> xs,
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector)
    {
        var result = new Dictionary<TKey, IList<TValue>>();

        foreach (var x in xs)
        {
            var key = keySelector(x);
            var value = valueSelector(x);

            if (result.TryGetValue(key, out var list))
                list.Add(value);
            else
                result[key] = new List<TValue> { value };
        }

        return result;
    }

    /// <summary>
    /// Convert to lookup-like dictionary, for JSON serialization
    /// </summary>
    /// <typeparam name="T">Source type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <param name="xs">List of objects</param>
    /// <param name="keySelector">A key-selector function</param>
    /// <returns>A dictionary with values grouped by key</returns>
    public static IDictionary<TKey, IList<T>> ToGroupedDictionary<T, TKey>(
        this IEnumerable<T> xs,
        Func<T, TKey> keySelector)
    {
        return xs.ToGroupedDictionary(keySelector, x => x);
    }
}