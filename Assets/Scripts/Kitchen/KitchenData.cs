using System;
using System.Collections.Generic;
using DesktopPet.Feeding;
using DesktopPet.Inventory;

namespace DesktopPet.Kitchen
{
    /// <summary>
    /// Identifies recipes supported by the kitchen module.
    /// </summary>
    public enum RecipeId
    {
        TomatoSoup,
        BakedPotato,
        StrawberryIceCream,
        Popcorn,
        PumpkinStew,
        GrapeDrink,
        TomatoFriedRice,
        CornFriedRice,
        PumpkinPorridge,
        TomatoPotatoStew,
        PotatoFlatbread,
        StrawberryJamBread,
        FruitPlatter,
        StrawberryCake,
        BlueberryPie,
        GrapeTart,
        HarvestPlatter,
        GardenRiceBowl
    }

    /// <summary>
    /// Identifies the progression tier of a recipe.
    /// </summary>
    public enum RecipeTier
    {
        Basic,
        Intermediate,
        Advanced
    }

    /// <summary>
    /// Defines an ingredient item and the amount required by a recipe.
    /// </summary>
    [Serializable]
    public class IngredientRequirement
    {
        public InventoryItemId itemId;
        public int amount;
    }

    /// <summary>
    /// Defines static balance and display data for a recipe.
    /// </summary>
    [Serializable]
    public class RecipeDefinition
    {
        public RecipeId id;
        public string displayName;
        public RecipeTier tier;
        public DishCategory category;
        public int unlockFarmLevel;
        public int cookMinutes;
        public int satietyRestore;
        public int happinessRestore;
        public int kitchenExperience;
        public IngredientRequirement[] ingredients;
        public string description;
    }

    /// <summary>
    /// Provides static lookup data and unlock checks for the kitchen module.
    /// </summary>
    public static class KitchenDatabase
    {
        private static readonly RecipeDefinition[] recipes =
        {
            Recipe(RecipeId.TomatoSoup, "番茄汤", RecipeTier.Basic, DishCategory.Soup, 1, 3, 12, 4, 10, "酸甜开胃的基础料理。", (InventoryItemId.Tomato, 1)),
            Recipe(RecipeId.BakedPotato, "烤土豆", RecipeTier.Basic, DishCategory.Snack, 1, 5, 15, 3, 10, "前期稳定恢复饱食度的基础料理。", (InventoryItemId.Potato, 1)),
            Recipe(RecipeId.StrawberryIceCream, "草莓冰淇淋", RecipeTier.Basic, DishCategory.Dessert, 2, 8, 10, 6, 10, "清爽甜品，开心值恢复更高。", (InventoryItemId.Strawberry, 1)),
            Recipe(RecipeId.Popcorn, "爆米花", RecipeTier.Basic, DishCategory.Snack, 3, 5, 11, 4, 10, "玉米线快速消耗料理。", (InventoryItemId.Corn, 1)),
            Recipe(RecipeId.PumpkinStew, "南瓜炖煮", RecipeTier.Basic, DishCategory.Soup, 4, 12, 16, 4, 10, "温暖的中前期轻量恢复料理。", (InventoryItemId.Pumpkin, 1)),
            Recipe(RecipeId.GrapeDrink, "葡萄果饮", RecipeTier.Basic, DishCategory.Drink, 7, 10, 9, 7, 10, "后期轻量饮品，开心值恢复较高。", (InventoryItemId.Grape, 1)),
            Recipe(RecipeId.TomatoFriedRice, "番茄炒饭", RecipeTier.Intermediate, DishCategory.Staple, 1, 12, 20, 6, 15, "最早开放的双材料主食菜。", (InventoryItemId.Rice, 1), (InventoryItemId.Tomato, 1)),
            Recipe(RecipeId.CornFriedRice, "玉米炒饭", RecipeTier.Intermediate, DishCategory.Staple, 3, 15, 21, 6, 15, "玉米线主力菜。", (InventoryItemId.Rice, 1), (InventoryItemId.Corn, 1)),
            Recipe(RecipeId.PumpkinPorridge, "南瓜粥", RecipeTier.Intermediate, DishCategory.Staple, 4, 15, 22, 5, 15, "中期稳定主力料理。", (InventoryItemId.Rice, 1), (InventoryItemId.Pumpkin, 1)),
            Recipe(RecipeId.TomatoPotatoStew, "番茄炖土豆", RecipeTier.Intermediate, DishCategory.VegetableDish, 4, 20, 19, 7, 15, "消耗基础蔬菜库存的双材料蔬菜菜。", (InventoryItemId.Tomato, 1), (InventoryItemId.Potato, 1)),
            Recipe(RecipeId.PotatoFlatbread, "土豆面饼", RecipeTier.Intermediate, DishCategory.Staple, 5, 18, 23, 5, 15, "小麦线基础面食。", (InventoryItemId.Wheat, 1), (InventoryItemId.Potato, 1)),
            Recipe(RecipeId.StrawberryJamBread, "草莓果酱面包", RecipeTier.Intermediate, DishCategory.Dessert, 5, 20, 20, 8, 15, "略贵的中级甜品菜。", (InventoryItemId.Wheat, 1), (InventoryItemId.Strawberry, 2)),
            Recipe(RecipeId.FruitPlatter, "水果拼盘", RecipeTier.Intermediate, DishCategory.Dessert, 6, 18, 18, 9, 15, "过渡到高级菜的水果料理。", (InventoryItemId.Strawberry, 1), (InventoryItemId.Blueberry, 2)),
            Recipe(RecipeId.StrawberryCake, "草莓蛋糕", RecipeTier.Advanced, DishCategory.Dessert, 5, 35, 30, 10, 25, "阶段性目标甜品。", (InventoryItemId.Wheat, 2), (InventoryItemId.Strawberry, 2)),
            Recipe(RecipeId.BlueberryPie, "蓝莓派", RecipeTier.Advanced, DishCategory.Dessert, 6, 40, 32, 10, 25, "后期高价值甜品。", (InventoryItemId.Blueberry, 2), (InventoryItemId.Wheat, 2)),
            Recipe(RecipeId.GrapeTart, "葡萄挞", RecipeTier.Advanced, DishCategory.Dessert, 7, 40, 31, 11, 25, "葡萄线毕业甜品。", (InventoryItemId.Grape, 2), (InventoryItemId.Wheat, 2)),
            Recipe(RecipeId.HarvestPlatter, "丰收拼盘", RecipeTier.Advanced, DishCategory.VegetableDish, 8, 45, 34, 12, 25, "综合高级菜，强调蔬菜和水果收集。", (InventoryItemId.Tomato, 2), (InventoryItemId.Strawberry, 2)),
            Recipe(RecipeId.GardenRiceBowl, "田园盖饭", RecipeTier.Advanced, DishCategory.Staple, 8, 30, 36, 9, 25, "主线毕业主食料理。", (InventoryItemId.Tomato, 2), (InventoryItemId.Rice, 2))
        };

        /// <summary>
        /// Gets all recipe definitions.
        /// </summary>
        public static IReadOnlyList<RecipeDefinition> Recipes => recipes;

        /// <summary>
        /// Gets a recipe definition by identifier.
        /// </summary>
        /// <param name="id">The recipe identifier.</param>
        /// <returns>The matching recipe definition.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the recipe identifier is unknown.</exception>
        public static RecipeDefinition GetRecipe(RecipeId id)
        {
            foreach (RecipeDefinition recipe in recipes)
            {
                if (recipe.id == id)
                {
                    return recipe;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown recipe id.");
        }

        /// <summary>
        /// Determines whether a recipe is unlocked by farm level.
        /// </summary>
        /// <param name="recipeId">The recipe to inspect.</param>
        /// <param name="farmLevel">The current farm level.</param>
        /// <returns><see langword="true"/> if the recipe is unlocked; otherwise, <see langword="false"/>.</returns>
        public static bool IsRecipeUnlocked(RecipeId recipeId, int farmLevel)
        {
            return farmLevel >= GetRecipe(recipeId).unlockFarmLevel;
        }

        private static RecipeDefinition Recipe(
            RecipeId id,
            string displayName,
            RecipeTier tier,
            DishCategory category,
            int unlockFarmLevel,
            int cookMinutes,
            int satietyRestore,
            int happinessRestore,
            int kitchenExperience,
            string description,
            params (InventoryItemId itemId, int amount)[] ingredients)
        {
            IngredientRequirement[] requirements = new IngredientRequirement[ingredients.Length];
            for (int i = 0; i < ingredients.Length; i++)
            {
                requirements[i] = new IngredientRequirement
                {
                    itemId = ingredients[i].itemId,
                    amount = ingredients[i].amount
                };
            }

            return new RecipeDefinition
            {
                id = id,
                displayName = displayName,
                tier = tier,
                category = category,
                unlockFarmLevel = unlockFarmLevel,
                cookMinutes = cookMinutes,
                satietyRestore = satietyRestore,
                happinessRestore = happinessRestore,
                kitchenExperience = kitchenExperience,
                ingredients = requirements,
                description = description
            };
        }
    }
}
