using System.Text.Json;
using System.Text.Json.Serialization;
using MealPlanner.Core;
using Microsoft.JSInterop;

namespace MealPlanner.App.Services;

/// <summary>Everything the app keeps, saved as one JSON document in the browser's local storage.</summary>
public sealed class AppData
{
    public int Version { get; set; } = 1;
    public List<Ingredient> Ingredients { get; set; } = [];
    public List<PantryItem> Pantry { get; set; } = [];
    public List<Recipe> Recipes { get; set; } = [];

    public static AppData CreateDefault() => new()
    {
        Ingredients = StarterData.Ingredients.ToList(),
        Recipes = StarterData.Recipes.ToList(),
    };
}

[JsonSourceGenerationOptions(UseStringEnumConverter = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(AppData))]
internal partial class AppJsonContext : JsonSerializerContext;

/// <summary>
/// Loads and saves the household's data on this device. Milestone 4 swaps local storage
/// for Supabase so both phones share it; the pages won't need to change.
/// </summary>
public sealed class AppStore(IJSRuntime js)
{
    private const string StorageKey = "meal-planner/v1";
    private Task? _loading;

    public AppData Data { get; private set; } = AppData.CreateDefault();
    public IngredientCatalog Catalog { get; private set; } = StarterData.CreateCatalog();

    public Task EnsureLoadedAsync() => _loading ??= LoadAsync();

    private async Task LoadAsync()
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                Data = JsonSerializer.Deserialize(json, AppJsonContext.Default.AppData) ?? AppData.CreateDefault();
            }
        }
        catch (JsonException)
        {
            Data = AppData.CreateDefault();
        }
        RebuildCatalog();
    }

    public async Task SaveAsync()
    {
        RebuildCatalog();
        var json = JsonSerializer.Serialize(Data, AppJsonContext.Default.AppData);
        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    public async Task ResetAsync()
    {
        Data = AppData.CreateDefault();
        await SaveAsync();
    }

    /// <summary>Canonical name for an ingredient, adding it to the catalog (section "Other") if it's new.</summary>
    public string EnsureIngredient(string name)
    {
        var canonical = Catalog.Resolve(name);
        if (!Catalog.Contains(name))
        {
            Data.Ingredients.Add(new Ingredient(canonical, StoreSection.Other));
            RebuildCatalog();
        }
        return canonical;
    }

    public PantryItem? FindPantry(string canonical) =>
        Data.Pantry.FirstOrDefault(p => Same(Catalog.Resolve(p.Ingredient), canonical));

    public void SetPantry(string name, StockLevel level, DateOnly? expiresOn)
    {
        var canonical = EnsureIngredient(name);
        RemovePantry(canonical);
        Data.Pantry.Add(new PantryItem(canonical, level, expiresOn));
    }

    public void RemovePantry(string canonical) =>
        Data.Pantry.RemoveAll(p => Same(Catalog.Resolve(p.Ingredient), canonical));

    private void RebuildCatalog() => Catalog = new IngredientCatalog(Data.Ingredients);

    private static bool Same(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
