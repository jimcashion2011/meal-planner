using MealPlanner.Core;

namespace PlannerConsole;

/// <summary>
/// The family's 14 starting dinners: five favorites plus nine weeknight recipes
/// (all 45 minutes or less) chosen to share ingredients with each other.
/// </summary>
public static class FamilyRecipes
{
    public static IngredientCatalog Catalog { get; } = new(
    [
        // Meat
        new Ingredient("chicken breast", StoreSection.MeatAndSeafood, Aliases: ["chicken", "chicken breasts"]),
        new Ingredient("chicken thigh", StoreSection.MeatAndSeafood),
        new Ingredient("ground beef", StoreSection.MeatAndSeafood, Aliases: ["hamburger meat", "beef mince"]),
        new Ingredient("chuck roast", StoreSection.MeatAndSeafood, Aliases: ["pot roast beef"]),
        new Ingredient("flank steak", StoreSection.MeatAndSeafood),
        new Ingredient("pepperoni", StoreSection.MeatAndSeafood),
        new Ingredient("bacon", StoreSection.MeatAndSeafood),
        new Ingredient("italian sausage", StoreSection.MeatAndSeafood),
        // Produce
        new Ingredient("broccoli", StoreSection.Produce),
        new Ingredient("bell pepper", StoreSection.Produce, Aliases: ["peppers"]),
        new Ingredient("onion", StoreSection.Produce, Aliases: ["yellow onion"]),
        new Ingredient("garlic", StoreSection.Produce, IsStaple: true),
        new Ingredient("carrot", StoreSection.Produce),
        new Ingredient("potato", StoreSection.Produce, Aliases: ["russet potato"]),
        new Ingredient("green bean", StoreSection.Produce),
        new Ingredient("zucchini", StoreSection.Produce),
        new Ingredient("lettuce", StoreSection.Produce),
        new Ingredient("tomato", StoreSection.Produce),
        new Ingredient("green onion", StoreSection.Produce, Aliases: ["scallion"]),
        // Dairy
        new Ingredient("mozzarella stick", StoreSection.Dairy, Aliases: ["string cheese"]),
        new Ingredient("shredded mozzarella", StoreSection.Dairy),
        new Ingredient("cheddar", StoreSection.Dairy, Aliases: ["american cheese", "cheese slices"]),
        new Ingredient("parmesan", StoreSection.Dairy),
        new Ingredient("heavy cream", StoreSection.Dairy),
        new Ingredient("butter", StoreSection.Dairy, IsStaple: true),
        new Ingredient("milk", StoreSection.Dairy),
        new Ingredient("egg", StoreSection.Dairy),
        new Ingredient("crescent roll dough", StoreSection.Dairy, Aliases: ["crescent rolls", "big & flaky crescent rolls"]),
        // Bakery
        new Ingredient("hamburger bun", StoreSection.Bakery),
        new Ingredient("tortilla", StoreSection.Bakery, Aliases: ["flour tortillas"]),
        // Frozen
        new Ingredient("frozen mixed vegetables", StoreSection.Frozen, Aliases: ["peas and carrots", "frozen peas and carrots"]),
        // Pantry
        new Ingredient("spaghetti", StoreSection.PantryDry),
        new Ingredient("penne", StoreSection.PantryDry),
        new Ingredient("fettuccine", StoreSection.PantryDry),
        new Ingredient("lasagna noodle", StoreSection.PantryDry),
        new Ingredient("honey", StoreSection.PantryDry),
        new Ingredient("taco seasoning", StoreSection.SpicesAndSauces, Aliases: ["fajita seasoning"]),
        new Ingredient("rice", StoreSection.PantryDry),
        new Ingredient("flour", StoreSection.PantryDry, IsStaple: true),
        new Ingredient("cornstarch", StoreSection.PantryDry, IsStaple: true),
        new Ingredient("pancake mix", StoreSection.Breakfast),
        new Ingredient("maple syrup", StoreSection.Breakfast),
        new Ingredient("beef broth", StoreSection.CannedAndJarred),
        new Ingredient("marinara", StoreSection.CannedAndJarred, Aliases: ["pasta sauce", "pizza sauce"]),
        // Spices and sauces
        new Ingredient("salt", StoreSection.SpicesAndSauces, IsStaple: true),
        new Ingredient("black pepper", StoreSection.SpicesAndSauces, IsStaple: true),
        new Ingredient("olive oil", StoreSection.SpicesAndSauces, IsStaple: true),
        new Ingredient("vegetable oil", StoreSection.SpicesAndSauces, IsStaple: true),
        new Ingredient("soy sauce", StoreSection.SpicesAndSauces, IsStaple: true),
        new Ingredient("italian seasoning", StoreSection.SpicesAndSauces, IsStaple: true),
        new Ingredient("orange chicken sauce", StoreSection.SpicesAndSauces),
        new Ingredient("orange", StoreSection.Produce),
        new Ingredient("brown sugar", StoreSection.PantryDry, IsStaple: true),
    ]);

    private static Recipe Make(string title, string protein, int minutes, string[] tags, params string[] ingredients) =>
        new(title, ingredients.Select(i => new RecipeIngredient(i)).ToList(), minutes, protein, tags);

    private static readonly string[] Kid = ["kid-friendly"];
    private static readonly string[] KidQuick = ["kid-friendly", "quick"];
    private static readonly string[] None = [];

    public static IReadOnlyList<Recipe> All { get; } =
    [
        // Named family favorites
        Make("Pizza roll-ups", "pepperoni", 25, KidQuick,
            "crescent roll dough", "mozzarella stick", "pepperoni", "marinara"),
        Make("Cheeseburgers", "beef", 25, KidQuick,
            "ground beef", "hamburger bun", "cheddar", "lettuce", "tomato", "salt", "black pepper"),
        Make("Pot roast", "beef", 240, None,
            "chuck roast", "potato", "carrot", "onion", "beef broth", "garlic", "salt", "black pepper"),
        Make("Orange chicken", "chicken", 40, Kid,
            "chicken breast", "orange chicken sauce", "rice", "broccoli", "cornstarch", "vegetable oil"),
        Make("Breakfast for dinner", "eggs", 25, KidQuick,
            "pancake mix", "egg", "bacon", "milk", "maple syrup", "butter"),

        // Pasta
        Make("Chicken and broccoli alfredo", "chicken", 30, Kid,
            "fettuccine", "chicken breast", "broccoli", "heavy cream", "parmesan", "butter", "garlic"),
        Make("Skillet lasagna", "beef", 30, Kid,
            "ground beef", "lasagna noodle", "marinara", "shredded mozzarella", "parmesan", "onion", "garlic"),
        Make("Sausage and pepper penne", "sausage", 25, KidQuick,
            "penne", "italian sausage", "bell pepper", "onion", "marinara"),

        // Chicken and vegetables
        Make("Sheet-pan chicken fajitas", "chicken", 30, KidQuick,
            "chicken breast", "bell pepper", "onion", "tortilla", "fajita seasoning", "vegetable oil"),
        Make("Chicken fried rice", "chicken", 25, KidQuick,
            "chicken breast", "rice", "egg", "peas and carrots", "soy sauce", "green onion"),
        Make("Honey-garlic chicken thighs with potatoes and green beans", "chicken", 40, Kid,
            "chicken thigh", "potato", "green bean", "honey", "soy sauce", "garlic", "olive oil"),

        // Beef and vegetables
        Make("Beef and broccoli", "beef", 30, None,
            "flank steak", "broccoli", "rice", "soy sauce", "brown sugar", "garlic", "cornstarch"),
        Make("Beef tacos", "beef", 20, KidQuick,
            "ground beef", "taco seasoning", "tortilla", "cheddar"),
        Make("Unstuffed pepper skillet", "beef", 30, Kid,
            "ground beef", "bell pepper", "rice", "marinara", "shredded mozzarella", "onion"),
    ];
}
