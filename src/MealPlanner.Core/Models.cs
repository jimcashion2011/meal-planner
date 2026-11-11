namespace MealPlanner.Core;

/// <summary>Rough pantry quantity. Planning drops an item one step per meal that uses it.</summary>
public enum StockLevel
{
    Out = 0,
    Low = 1,
    Some = 2,
    Plenty = 3
}

public enum StoreSection
{
    Produce,
    MeatAndSeafood,
    Dairy,
    Bakery,
    Frozen,
    PantryDry,
    CannedAndJarred,
    SpicesAndSauces,
    Breakfast,
    Other
}

/// <summary>One canonical ingredient. Aliases map other spellings onto it ("scallion" = "green onion").</summary>
public sealed record Ingredient(
    string CanonicalName,
    StoreSection Section,
    bool IsStaple = false,
    IReadOnlyList<string>? Aliases = null);

public sealed record PantryItem(
    string Ingredient,
    StockLevel Level,
    DateOnly? ExpiresOn = null);

public sealed record RecipeIngredient(
    string Ingredient,
    string AmountText = "",
    bool IsOptional = false);

public sealed record Recipe(
    string Title,
    IReadOnlyList<RecipeIngredient> Ingredients,
    int PrepMinutes,
    string MainProtein,
    IReadOnlyList<string>? Tags = null,
    DateOnly? LastMadeOn = null);

public sealed record PlannedMeal(
    DateOnly Date,
    Recipe? Recipe,
    double Score,
    bool Locked,
    IReadOnlyList<string> MissingIngredients)
{
    public bool IsLeftoversNight => Recipe is null;
}

public sealed record GroceryItem(
    string Ingredient,
    StoreSection Section,
    IReadOnlyList<string> ForRecipes);

/// <summary>Settings for one weekly plan.</summary>
public sealed class PlanOptions
{
    public required DateOnly WeekStart { get; init; }
    public int Days { get; init; } = 7;

    /// <summary>Dates left open for leftovers.</summary>
    public IReadOnlySet<DateOnly> LeftoversNights { get; init; } = new HashSet<DateOnly>();

    /// <summary>Recipes fixed to a date before scoring starts.</summary>
    public IReadOnlyDictionary<DateOnly, Recipe> LockedMeals { get; init; } = new Dictionary<DateOnly, Recipe>();

    /// <summary>Tags the family prefers this week, e.g. "kid-friendly".</summary>
    public IReadOnlyList<string> PreferredTags { get; init; } = [];

    /// <summary>Busy nights get a prep-time cap.</summary>
    public IReadOnlyDictionary<DayOfWeek, int> MaxPrepMinutesByDay { get; init; } = new Dictionary<DayOfWeek, int>();

    /// <summary>Pantry items expiring within this many days of WeekStart count as "use soon".</summary>
    public int ExpiringWithinDays { get; init; } = 4;

    /// <summary>Recipes made within this many days before WeekStart count as repeats.</summary>
    public int RepeatWindowDays { get; init; } = 14;

    public ScoreWeights Weights { get; init; } = new();
}

/// <summary>score = Coverage*C + Expiry*E + Fit*F - Missing*M - Repetition*R. Starting values from the spec.</summary>
public sealed record ScoreWeights(
    double Coverage = 3.0,
    double Expiry = 2.0,
    double Fit = 1.0,
    double Missing = 1.5,
    double Repetition = 2.0);

public sealed record ScoreBreakdown(
    double Coverage,
    double Expiry,
    double Fit,
    double Missing,
    double Repetition,
    double Total,
    IReadOnlyList<string> MissingIngredients);

public sealed record WeeklyPlan(
    IReadOnlyList<PlannedMeal> Meals,
    IReadOnlyList<GroceryItem> GroceryList,
    IReadOnlyDictionary<string, StockLevel> PantryAfterPlan);
