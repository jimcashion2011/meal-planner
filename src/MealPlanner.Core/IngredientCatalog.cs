namespace MealPlanner.Core;

/// <summary>
/// Maps every spelling of an ingredient to one canonical name, so pantry, recipes
/// and the grocery list all agree. Unknown names are kept as their own canonical entry.
/// </summary>
public sealed class IngredientCatalog
{
    private readonly Dictionary<string, Ingredient> _byKey = new(StringComparer.OrdinalIgnoreCase);

    public IngredientCatalog(IEnumerable<Ingredient> ingredients)
    {
        foreach (var ingredient in ingredients)
        {
            Add(ingredient);
        }
    }

    public void Add(Ingredient ingredient)
    {
        foreach (var name in (ingredient.Aliases ?? []).Prepend(ingredient.CanonicalName))
        {
            _byKey[Clean(name)] = ingredient;
            _byKey[Normalize(name)] = ingredient;
        }
    }

    /// <summary>Canonical name for any spelling; unknown names come back normalized.</summary>
    public string Resolve(string name) => Get(name).CanonicalName;

    public Ingredient Get(string name)
    {
        if (_byKey.TryGetValue(Clean(name), out var exact)) return exact;
        if (_byKey.TryGetValue(Normalize(name), out var singular)) return singular;
        return new Ingredient(Normalize(name), StoreSection.Other);
    }

    public bool IsStaple(string name) => Get(name).IsStaple;

    /// <summary>Every known ingredient once, sorted by name.</summary>
    public IReadOnlyList<Ingredient> All =>
        _byKey.Values.Distinct().OrderBy(i => i.CanonicalName, StringComparer.Ordinal).ToList();

    public bool Contains(string name) => _byKey.ContainsKey(Clean(name)) || _byKey.ContainsKey(Normalize(name));

    /// <summary>Lowercase, trimmed, single spaces.</summary>
    public static string Clean(string name) =>
        string.Join(' ', name.Trim().ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Clean, then a simple singular: "potatoes" to "potato", "eggs" to "egg".</summary>
    public static string Normalize(string name)
    {
        var cleaned = Clean(name);
        if (cleaned.Length > 4 && cleaned.EndsWith("oes")) return cleaned[..^2];
        if (cleaned.Length > 3 && cleaned.EndsWith('s') && !cleaned.EndsWith("ss")) return cleaned[..^1];
        return cleaned;
    }
}
