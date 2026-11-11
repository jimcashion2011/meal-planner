using MealPlanner.Core;

namespace MealPlanner.Core.Tests;

public class PlannerTests
{
    private static readonly DateOnly Monday = new(2027, 2, 1);

    private static readonly IngredientCatalog Catalog = new(
    [
        new Ingredient("chicken breast", StoreSection.MeatAndSeafood, Aliases: ["chicken"]),
        new Ingredient("ground beef", StoreSection.MeatAndSeafood, Aliases: ["hamburger meat"]),
        new Ingredient("broccoli", StoreSection.Produce),
        new Ingredient("green onion", StoreSection.Produce, Aliases: ["scallion"]),
        new Ingredient("potato", StoreSection.Produce),
        new Ingredient("spaghetti", StoreSection.PantryDry),
        new Ingredient("marinara", StoreSection.CannedAndJarred),
        new Ingredient("salt", StoreSection.SpicesAndSauces, IsStaple: true),
        new Ingredient("olive oil", StoreSection.SpicesAndSauces, IsStaple: true),
    ]);

    private static Recipe R(string title, string protein, params string[] ingredients) =>
        new(title, ingredients.Select(i => new RecipeIngredient(i)).ToList(), PrepMinutes: 30, MainProtein: protein);

    private static PlanOptions Week(int days = 7) => new() { WeekStart = Monday, Days = days };

    [Fact]
    public void Catalog_resolves_aliases_and_plurals()
    {
        Assert.Equal("green onion", Catalog.Resolve("Scallions"));
        Assert.Equal("potato", Catalog.Resolve("potatoes"));
        Assert.Equal("chicken breast", Catalog.Resolve("  Chicken "));
        Assert.Equal("egg", Catalog.Resolve("Eggs")); // unknown names still normalize
    }

    [Fact]
    public void Prefers_the_recipe_we_already_have_ingredients_for()
    {
        var recipes = new[]
        {
            R("Beef spaghetti", "beef", "ground beef", "spaghetti", "marinara"),
            R("Chicken and broccoli", "chicken", "chicken", "broccoli", "olive oil", "salt"),
        };
        var pantry = new[]
        {
            new PantryItem("chicken breast", StockLevel.Some),
            new PantryItem("broccoli", StockLevel.Some),
        };

        var plan = new Planner(Catalog).Plan(recipes, pantry, Week(days: 1));

        Assert.Equal("Chicken and broccoli", plan.Meals[0].Recipe!.Title);
        Assert.Empty(plan.Meals[0].MissingIngredients);
    }

    [Fact]
    public void Two_meals_cannot_both_count_on_the_last_of_an_ingredient()
    {
        var recipes = new[]
        {
            R("Chicken A", "chicken", "chicken", "broccoli"),
            R("Chicken B", "chicken", "chicken", "potato"),
        };
        var pantry = new[]
        {
            new PantryItem("chicken", StockLevel.Low),
            new PantryItem("broccoli", StockLevel.Some),
            new PantryItem("potato", StockLevel.Some),
        };

        var plan = new Planner(Catalog).Plan(recipes, pantry, Week(days: 2));

        Assert.Empty(plan.Meals[0].MissingIngredients);
        Assert.Contains("chicken breast", plan.Meals[1].MissingIngredients);
        Assert.Equal(StockLevel.Out, plan.PantryAfterPlan["chicken breast"]);
    }

    [Fact]
    public void Expiring_items_pull_their_recipe_forward()
    {
        var recipes = new[]
        {
            R("Uses broccoli", "chicken", "chicken", "broccoli"),
            R("Uses potato", "chicken", "chicken", "potato"),
        };
        var pantry = new[]
        {
            new PantryItem("chicken", StockLevel.Plenty),
            new PantryItem("broccoli", StockLevel.Some),
            new PantryItem("potato", StockLevel.Some, ExpiresOn: Monday.AddDays(2)),
        };

        var plan = new Planner(Catalog).Plan(recipes, pantry, Week(days: 1));

        Assert.Equal("Uses potato", plan.Meals[0].Recipe!.Title);
    }

    [Fact]
    public void Staples_are_assumed_unless_marked_out()
    {
        var recipe = R("Roast potatoes", "none", "potato", "olive oil", "salt");
        var pantry = new[]
        {
            new PantryItem("potato", StockLevel.Some),
            new PantryItem("olive oil", StockLevel.Out),
        };

        var plan = new Planner(Catalog).Plan([recipe], pantry, Week(days: 1));

        Assert.Equal(new[] { "olive oil" }, plan.Meals[0].MissingIngredients);
        Assert.Single(plan.GroceryList);
    }

    [Fact]
    public void Locked_meals_and_leftovers_nights_are_respected()
    {
        var locked = R("Pot roast", "beef", "ground beef", "potato");
        var recipes = new[] { locked, R("Spaghetti", "beef", "spaghetti", "marinara") };
        var options = new PlanOptions
        {
            WeekStart = Monday,
            Days = 3,
            LockedMeals = new Dictionary<DateOnly, Recipe> { [Monday.AddDays(2)] = locked },
            LeftoversNights = new HashSet<DateOnly> { Monday.AddDays(1) },
        };

        var plan = new Planner(Catalog).Plan(recipes, [], options);

        Assert.Equal("Spaghetti", plan.Meals[0].Recipe!.Title);
        Assert.True(plan.Meals[1].IsLeftoversNight);
        Assert.True(plan.Meals[2].Locked);
        Assert.Equal("Pot roast", plan.Meals[2].Recipe!.Title);
    }

    [Fact]
    public void Avoids_the_same_protein_two_nights_running()
    {
        var recipes = new[]
        {
            R("Chicken 1", "chicken", "chicken"),
            R("Chicken 2", "chicken", "chicken"),
            R("Beef 1", "beef", "ground beef"),
            R("Beef 2", "beef", "ground beef"),
        };
        var pantry = new[]
        {
            new PantryItem("chicken", StockLevel.Plenty),
            new PantryItem("ground beef", StockLevel.Plenty),
        };

        var plan = new Planner(Catalog).Plan(recipes, pantry, Week(days: 3));

        Assert.NotEqual(plan.Meals[0].Recipe!.MainProtein, plan.Meals[1].Recipe!.MainProtein);
        Assert.NotEqual(plan.Meals[1].Recipe!.MainProtein, plan.Meals[2].Recipe!.MainProtein);
    }

    [Fact]
    public void Grocery_list_merges_and_groups_by_section()
    {
        var recipes = new[]
        {
            R("Spaghetti", "beef", "ground beef", "spaghetti", "marinara"),
            R("Burgers", "beef", "hamburger meat", "potato"),
        };

        var plan = new Planner(Catalog).Plan(recipes, [], Week(days: 2));

        var beef = Assert.Single(plan.GroceryList, g => g.Ingredient == "ground beef");
        Assert.Equal(2, beef.ForRecipes.Count);
        Assert.Equal(StoreSection.Produce, plan.GroceryList[0].Section);
    }

    [Fact]
    public void Busy_night_prep_cap_is_respected_when_possible()
    {
        var quick = new Recipe("Quick tacos", [new RecipeIngredient("ground beef")], PrepMinutes: 20, MainProtein: "beef");
        var slow = new Recipe("Slow roast", [new RecipeIngredient("ground beef"), new RecipeIngredient("potato")], PrepMinutes: 180, MainProtein: "beef");
        var pantry = new[] { new PantryItem("ground beef", StockLevel.Plenty), new PantryItem("potato", StockLevel.Plenty) };
        var options = new PlanOptions
        {
            WeekStart = Monday,
            Days = 1,
            MaxPrepMinutesByDay = new Dictionary<DayOfWeek, int> { [DayOfWeek.Monday] = 30 },
        };

        var plan = new Planner(Catalog).Plan([quick, slow], pantry, options);

        Assert.Equal("Quick tacos", plan.Meals[0].Recipe!.Title);
    }
}
