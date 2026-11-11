# Pantry-First Meal Planner: planner core

Milestone 1 from the product spec: the meal-picking logic as a plain .NET library,
with unit tests and a console sample using the family's starting recipes.

Spec: https://claude.ai/artifact/PJFbBHxfXN5WfdMBdHLdtD

## Layout

```
src/MealPlanner.Core/           the library (reused later by the Blazor app and a MAUI app)
  Models.cs                     pantry, recipe, plan types and planning options
  IngredientCatalog.cs          canonical names + aliases ("scallion" = "green onion")
  Planner.cs                    scoring, reservations, grocery list
tests/MealPlanner.Core.Tests/   xUnit tests for the rules in the spec
samples/PlannerConsole/         prints a sample week from FamilyRecipes.cs
```

## Build and run

Requires the .NET 10 SDK (https://dotnet.microsoft.com/download).

```
dotnet test tests/MealPlanner.Core.Tests
dotnet run --project samples/PlannerConsole
```

Status: written without a compiler available, so the first build may surface small fixes.

## How a plan is chosen

For each night, every unused recipe is scored:

score = 3*Coverage + 2*Expiry + 1*Fit - 1.5*Missing - 2*Repetition

- Coverage: share of non-staple ingredients on hand
- Expiry: share of soon-to-expire pantry items the recipe uses
- Fit: share of preferred tags matched (halved if over the night's prep cap)
- Missing: ingredients to buy / 5
- Repetition: made in the last 14 days, or same main protein as the night before

The top recipe is picked and its on-hand ingredients drop one level
(Plenty, Some, Low, Out), so two meals can't both count on the last of something.
Locked meals are placed first; leftovers nights are skipped.
Staples (salt, oil, garlic...) are assumed on hand unless the pantry marks them Out.

## Next steps

1. Replace the `[Pasta 1]`, `[Chicken 1]`, `[Beef 1]` placeholder recipes in
   `samples/PlannerConsole/FamilyRecipes.cs` with the family's real dishes.
2. Run the sample and tune the weights in `ScoreWeights` if the picks feel off.
3. Milestone 2: Blazor WebAssembly app with pantry and recipe screens.
