using MealPlanner.Core;

// A sample week: a realistic pantry, leftovers on Thursday, quick dinners on weeknights.
var weekStart = new DateOnly(2027, 2, 1); // a Monday

var pantry = new List<PantryItem>
{
    new("chicken breast", StockLevel.Some, ExpiresOn: weekStart.AddDays(2)),
    new("ground beef", StockLevel.Plenty),
    new("broccoli", StockLevel.Some, ExpiresOn: weekStart.AddDays(3)),
    new("rice", StockLevel.Plenty),
    new("spaghetti", StockLevel.Some),
    new("marinara", StockLevel.Some),
    new("onion", StockLevel.Some),
    new("egg", StockLevel.Plenty),
    new("milk", StockLevel.Some),
    new("pancake mix", StockLevel.Some),
    new("cheddar", StockLevel.Low),
    new("frozen mixed vegetables", StockLevel.Some),
    new("string cheese", StockLevel.Some),
    new("potatoes", StockLevel.Some),
};

var options = new PlanOptions
{
    WeekStart = weekStart,
    LeftoversNights = new HashSet<DateOnly> { weekStart.AddDays(3) },
    PreferredTags = ["kid-friendly"],
    MaxPrepMinutesByDay = new Dictionary<DayOfWeek, int>
    {
        [DayOfWeek.Monday] = 30,
        [DayOfWeek.Tuesday] = 30,
        [DayOfWeek.Wednesday] = 30,
        [DayOfWeek.Thursday] = 30,
        [DayOfWeek.Friday] = 30,
    },
};

var plan = new Planner(StarterData.CreateCatalog()).Plan(StarterData.Recipes, pantry, options);

Console.WriteLine($"Dinner plan, week of {weekStart:MMM d}");
Console.WriteLine(new string('-', 60));
foreach (var meal in plan.Meals)
{
    var title = meal.IsLeftoversNight ? "Leftovers" : meal.Recipe!.Title;
    var missing = meal.MissingIngredients.Count == 0 ? "" : $"  (buy: {string.Join(", ", meal.MissingIngredients)})";
    Console.WriteLine($"{meal.Date:ddd}  {title,-42} {meal.Score,6:0.00}{missing}");
}

Console.WriteLine();
Console.WriteLine("Grocery list");
Console.WriteLine(new string('-', 60));
foreach (var section in plan.GroceryList.GroupBy(g => g.Section))
{
    Console.WriteLine(section.Key);
    foreach (var item in section)
    {
        Console.WriteLine($"  [ ] {item.Ingredient}  ({string.Join(", ", item.ForRecipes)})");
    }
}
