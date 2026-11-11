namespace MealPlanner.Core;

/// <summary>
/// Builds a weekly dinner plan from what is on hand.
/// Greedy: score every recipe against the current pantry, pick the best,
/// reserve its ingredients (drop each one level), then rescore for the next night.
/// </summary>
public sealed class Planner(IngredientCatalog catalog)
{
    private readonly IngredientCatalog _catalog = catalog;

    public WeeklyPlan Plan(IReadOnlyList<Recipe> recipes, IReadOnlyList<PantryItem> pantry, PlanOptions options)
    {
        var levels = BuildLevels(pantry);
        var expiring = FindExpiring(pantry, options);
        var dates = Enumerable.Range(0, options.Days).Select(options.WeekStart.AddDays).ToList();
        var meals = new Dictionary<DateOnly, PlannedMeal>();
        var usedTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Locked meals claim their ingredients first.
        foreach (var date in dates)
        {
            if (!options.LockedMeals.TryGetValue(date, out var locked)) continue;
            var score = Score(locked, levels, expiring, options, date, previousProtein: null);
            Reserve(locked, levels, expiring);
            meals[date] = new PlannedMeal(date, locked, score.Total, Locked: true, score.MissingIngredients);
            usedTitles.Add(locked.Title);
        }

        // 2. Fill the remaining nights in date order.
        string? previousProtein = null;
        foreach (var date in dates)
        {
            if (meals.TryGetValue(date, out var existing))
            {
                previousProtein = existing.Recipe?.MainProtein ?? previousProtein;
                continue;
            }

            if (options.LeftoversNights.Contains(date))
            {
                meals[date] = new PlannedMeal(date, null, 0, Locked: false, []);
                continue;
            }

            var candidates = recipes.Where(r => !usedTitles.Contains(r.Title)).ToList();
            var withinTime = candidates.Where(r => FitsPrepLimit(r, date, options)).ToList();
            if (withinTime.Count > 0) candidates = withinTime; // fall back to any recipe if none fit the cap

            var best = candidates
                .Select(r => (Recipe: r, Score: Score(r, levels, expiring, options, date, previousProtein)))
                .OrderByDescending(x => x.Score.Total)
                .ThenBy(x => x.Recipe.Title, StringComparer.Ordinal)
                .FirstOrDefault();

            if (best.Recipe is null)
            {
                meals[date] = new PlannedMeal(date, null, 0, Locked: false, []);
                continue;
            }

            Reserve(best.Recipe, levels, expiring);
            meals[date] = new PlannedMeal(date, best.Recipe, best.Score.Total, Locked: false, best.Score.MissingIngredients);
            usedTitles.Add(best.Recipe.Title);
            previousProtein = best.Recipe.MainProtein;
        }

        var ordered = dates.Select(d => meals[d]).ToList();
        return new WeeklyPlan(ordered, BuildGroceryList(ordered), levels);
    }

    /// <summary>score = wC*C + wE*E + wF*F - wM*M - wR*R (see the spec's Planning logic section).</summary>
    public ScoreBreakdown Score(
        Recipe recipe,
        IReadOnlyDictionary<string, StockLevel> levels,
        IReadOnlySet<string> expiring,
        PlanOptions options,
        DateOnly date,
        string? previousProtein)
    {
        var required = recipe.Ingredients
            .Where(i => !i.IsOptional)
            .Select(i => _catalog.Resolve(i.Ingredient))
            .Distinct()
            .ToList();
        var allNames = recipe.Ingredients
            .Select(i => _catalog.Resolve(i.Ingredient))
            .Distinct()
            .ToList();

        var missing = required.Where(n => !IsAvailable(n, levels)).ToList();
        var nonStaples = required.Where(n => !_catalog.IsStaple(n)).ToList();
        var nonStaplesOnHand = nonStaples.Count(n => IsAvailable(n, levels));

        double coverage = nonStaples.Count == 0 ? 1.0 : (double)nonStaplesOnHand / nonStaples.Count;

        double expiry = expiring.Count == 0
            ? 0.0
            : (double)allNames.Count(n => expiring.Contains(n) && IsAvailable(n, levels)) / expiring.Count;

        double fit = FitScore(recipe, date, options);

        double missingTerm = missing.Count / 5.0;

        bool madeRecently = recipe.LastMadeOn is { } last
            && last >= options.WeekStart.AddDays(-options.RepeatWindowDays)
            && last < options.WeekStart;
        bool sameProtein = previousProtein is not null
            && string.Equals(previousProtein, recipe.MainProtein, StringComparison.OrdinalIgnoreCase);
        double repetition = madeRecently || sameProtein ? 1.0 : 0.0;

        var w = options.Weights;
        double total = w.Coverage * coverage
                     + w.Expiry * expiry
                     + w.Fit * fit
                     - w.Missing * missingTerm
                     - w.Repetition * repetition;

        return new ScoreBreakdown(coverage, expiry, fit, missingTerm, repetition, Math.Round(total, 3), missing);
    }

    /// <summary>Share of preferred tags matched, halved if the recipe runs over that night's prep cap.</summary>
    private static double FitScore(Recipe recipe, DateOnly date, PlanOptions options)
    {
        double tagFit = 1.0;
        if (options.PreferredTags.Count > 0)
        {
            var tags = new HashSet<string>(recipe.Tags ?? [], StringComparer.OrdinalIgnoreCase);
            tagFit = (double)options.PreferredTags.Count(tags.Contains) / options.PreferredTags.Count;
        }
        return FitsPrepLimit(recipe, date, options) ? tagFit : tagFit * 0.5;
    }

    private static bool FitsPrepLimit(Recipe recipe, DateOnly date, PlanOptions options) =>
        !options.MaxPrepMinutesByDay.TryGetValue(date.DayOfWeek, out var cap) || recipe.PrepMinutes <= cap;

    /// <summary>Tracked items count if above Out. Untracked staples are assumed on hand.</summary>
    private bool IsAvailable(string canonical, IReadOnlyDictionary<string, StockLevel> levels) =>
        levels.TryGetValue(canonical, out var level) ? level > StockLevel.Out : _catalog.IsStaple(canonical);

    /// <summary>Each required, tracked, on-hand ingredient drops one level. Used items stop counting as "expiring".</summary>
    private void Reserve(Recipe recipe, Dictionary<string, StockLevel> levels, HashSet<string> expiring)
    {
        foreach (var name in recipe.Ingredients.Where(i => !i.IsOptional).Select(i => _catalog.Resolve(i.Ingredient)).Distinct())
        {
            if (levels.TryGetValue(name, out var level) && level > StockLevel.Out)
            {
                levels[name] = level - 1;
                expiring.Remove(name);
            }
        }
    }

    private Dictionary<string, StockLevel> BuildLevels(IReadOnlyList<PantryItem> pantry)
    {
        var levels = new Dictionary<string, StockLevel>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in pantry)
        {
            var name = _catalog.Resolve(item.Ingredient);
            // If the same ingredient is entered twice, keep the higher level.
            levels[name] = levels.TryGetValue(name, out var existing) && existing > item.Level ? existing : item.Level;
        }
        return levels;
    }

    private HashSet<string> FindExpiring(IReadOnlyList<PantryItem> pantry, PlanOptions options)
    {
        var cutoff = options.WeekStart.AddDays(options.ExpiringWithinDays);
        return pantry
            .Where(p => p.Level > StockLevel.Out && p.ExpiresOn is { } d && d <= cutoff)
            .Select(p => _catalog.Resolve(p.Ingredient))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private List<GroceryItem> BuildGroceryList(IReadOnlyList<PlannedMeal> meals) =>
        meals
            .Where(m => m.Recipe is not null)
            .SelectMany(m => m.MissingIngredients.Select(name => (Name: name, Recipe: m.Recipe!.Title)))
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroceryItem(
                g.Key,
                _catalog.Get(g.Key).Section,
                g.Select(x => x.Recipe).Distinct().ToList()))
            .OrderBy(item => item.Section)
            .ThenBy(item => item.Ingredient, StringComparer.Ordinal)
            .ToList();
}
