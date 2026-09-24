using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace psms.Infrastructure.Querying;

/// <summary>
/// Applies a client-supplied sort to a query safely.
/// <para>
/// A list screen sorts by the column it is showing, and those columns are
/// <b>DTO</b> fields — <c>termName</c>, <c>studentName</c>, <c>className</c> —
/// flattened from navigations by AutoMapper. <c>OrderBy(string)</c> runs against
/// the <b>entity</b>, where no such property exists, so dynamic LINQ threw a
/// parse exception and the request came back as a 500 "An internal error
/// occurred during your request!". Every list in the product behaved this way:
/// clicking a column header on a flattened column broke the screen.
/// </para>
/// <para>
/// Two things fix that. A per-service map translates the DTO field names its
/// screen sorts by into entity paths; and anything still unrecognised is
/// dropped rather than handed to the parser, so an unknown or malformed sort
/// falls back to the list's default order instead of failing the request. That
/// also means a caller cannot feed arbitrary dynamic-LINQ expressions through
/// this parameter.
/// </para>
/// </summary>
public static class SortingExtensions
{
    /// <summary>
    /// Orders by <paramref name="sorting"/> ("field asc, other desc"),
    /// translating DTO field names through <paramref name="map"/> and falling
    /// back to <paramref name="fallback"/> when nothing usable remains.
    /// </summary>
    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        string sorting,
        string fallback,
        IReadOnlyDictionary<string, string> map = null)
    {
        var resolved = ResolveSorting<T>(sorting, map);
        return query.OrderBy(resolved ?? fallback);
    }

    /// <summary>
    /// The entity-side sort string, or null when the request named nothing this
    /// entity can actually be ordered by. Exposed for tests.
    /// </summary>
    public static string ResolveSorting<T>(string sorting, IReadOnlyDictionary<string, string> map = null)
    {
        if (string.IsNullOrWhiteSpace(sorting)) return null;

        var terms = new List<string>();

        foreach (var raw in sorting.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = raw.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            var field = parts[0];
            var descending = parts.Length > 1 && parts[1].StartsWith("desc", StringComparison.OrdinalIgnoreCase);

            var path = Translate<T>(field, map);
            if (path == null) continue;   // not something this entity can be ordered by

            // A mapped column can expand to several entity columns — studentName
            // becomes "Student.LastName, Student.FirstName". The direction has to
            // be applied to each of them: appending it once to the whole string
            // left every column but the last sorted ascending, so "descending"
            // returned the same order as "ascending" and the header looked dead.
            var direction = descending ? "DESC" : "ASC";
            terms.AddRange(path
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(column => $"{column.Trim()} {direction}"));
        }

        return terms.Count > 0 ? string.Join(", ", terms) : null;
    }

    private static string Translate<T>(string field, IReadOnlyDictionary<string, string> map)
    {
        if (map != null && map.TryGetValue(field, out var mapped))
            return mapped;

        // A caller may legitimately pass an entity path already ("Student.LastName"),
        // which is what the services' own default sorts use.
        return PathExists(typeof(T), field) ? field : null;
    }

    /// <summary>
    /// Whether a dotted property path resolves on the entity. Reflection rather
    /// than a try/catch around the parser: a swallowed exception would also hide
    /// a genuinely broken query.
    /// </summary>
    private static bool PathExists(Type type, string path)
    {
        var current = type;

        foreach (var segment in path.Split('.'))
        {
            if (string.IsNullOrWhiteSpace(segment)) return false;

            var property = current.GetProperty(
                segment,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property == null) return false;
            current = property.PropertyType;
        }

        return true;
    }
}
