using MealPlanner.Core;

namespace PlannerConsole;

/// <summary>
/// Starting set from the spec's open questions. Named dishes come from Jim's list;
/// the pasta, chicken/vegetable and beef/vegetable slots are placeholders to swap
/// for the family's real recipes.
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
        // Frozen
        new Ingredient("frozen mixed vegetables", StoreSection.Frozen),
        // Pantry
        new Ingredient("spaghetti", StoreSection.PantryDry),
        new Ingredient("penne", StoreSection.PantryDry),
        new Ingredient("fettuccine", StoreSection.PantryDry),
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

        // Pasta placeholders
        Make("[Pasta 1] Spaghetti and meat sauce", "beef", 30, Kid,
            "spaghetti", "ground beef", "marinara", "onion", "garlic", "parmesan"),
        Make("[Pasta 2] Chicken fettuccine alfredo", "chicken", 30, Kid,
            "fettuccine", "chicken breast", "heavy cream", "parmesan", "butter", "garlic", "broccoli"),
        Make("[Pasta 3] Baked ziti with sausage", "sausage", 45, Kid,
            "penne", "italian sausage", "marinara", "shredded mozzarella", "parmesan"),

        // Chicken and vegetable placeholders
        Make("[Chicken 1] Sheet-pan chicken and veggies", "chicken", 40, None,
            "chicken thigh", "potato", "green bean", "olive oil", "garlic", "italian seasoning"),
        Make("[Chicken 2] Chicken fajitas", "chicken", 30, KidQuick,
            "chicken breast", "bell pepper", "onion", "vegetable oil"),
        Make("[Chicken 3] Chicken fried rice", "chicken", 25, KidQuick,
            "chicken breast", "rice", "egg", "frozen mixed vegetables", "soy sauce", "green onion"),

        // Beef and vegetable placeholders
        Make("[Beef 1] Beef and broccoli", "beef", 30, None,
            "flank steak", "broccoli", "rice", "soy sauce", "brown sugar", "garlic", "cornstarch"),
        Make("[Beef 2] Beef tacos", "beef", 20, KidQuick,
            "ground beef", "cheddar", "lettuce", "tomato", "onion"),
        Make("[Beef 3] Stuffed peppers", "beef", 50, None,
            "ground beef", "bell pepper", "rice", "marinara", "shredded mozzarella"),
    ];
}
