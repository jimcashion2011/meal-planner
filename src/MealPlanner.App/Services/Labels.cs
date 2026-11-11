using MealPlanner.Core;

namespace MealPlanner.App.Services;

public static class Labels
{
    public static readonly StockLevel[] Levels = [StockLevel.Out, StockLevel.Low, StockLevel.Some, StockLevel.Plenty];

    public static string Level(StockLevel level) => level switch
    {
        StockLevel.Out => "Out",
        StockLevel.Low => "Low",
        StockLevel.Some => "Some",
        _ => "Plenty",
    };

    public static string Section(StoreSection section) => section switch
    {
        StoreSection.Produce => "Produce",
        StoreSection.MeatAndSeafood => "Meat & seafood",
        StoreSection.Dairy => "Dairy & eggs",
        StoreSection.Bakery => "Bakery",
        StoreSection.Frozen => "Frozen",
        StoreSection.PantryDry => "Pantry",
        StoreSection.CannedAndJarred => "Canned & jarred",
        StoreSection.SpicesAndSauces => "Spices & sauces",
        StoreSection.Breakfast => "Breakfast",
        _ => "Other",
    };

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    public static bool UseSoon(DateOnly date) => date <= Today.AddDays(4);
}
