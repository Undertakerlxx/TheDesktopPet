using System;
using System.Collections.Generic;
using System.IO;
using DesktopPet.Catalog;
using DesktopPet.Farm;
using DesktopPet.Inventory;
using DesktopPet.Kitchen;
using UnityEngine;

namespace DesktopPet.Progress
{
    /// <summary>
    /// Loads, saves, and mutates persistent progress shared by farm, kitchen, storage, and catalog systems.
    /// </summary>
    public class DesktopPetProgressService
    {
        private const string SaveFileName = "farm-kitchen-catalog-progress.json";

        private readonly string savePath;

        /// <summary>
        /// Gets the currently loaded progress data.
        /// </summary>
        public DesktopPetProgressData Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopPetProgressService"/> class and loads saved progress.
        /// </summary>
        public DesktopPetProgressService()
        {
            savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
            Data = Load();
        }

        /// <summary>
        /// Loads progress from disk, creating default data when no save exists or loading fails.
        /// </summary>
        /// <returns>The loaded or default progress data.</returns>
        public DesktopPetProgressData Load()
        {
            if (!File.Exists(savePath))
            {
                return CreateDefaultData();
            }

            try
            {
                string json = File.ReadAllText(savePath);
                DesktopPetProgressData data = JsonUtility.FromJson<DesktopPetProgressData>(json);
                if (data == null)
                {
                    return CreateDefaultData();
                }

                data.EnsureCollections();
                data.EnsurePlotCount();
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"DesktopPetProgressService: failed to load progress. {exception.Message}");
                return CreateDefaultData();
            }
        }

        /// <summary>
        /// Reloads progress from disk so different UI windows can observe the latest saved state.
        /// </summary>
        public void Reload()
        {
            Data = Load();
        }

        /// <summary>
        /// Saves the current progress data to disk as JSON.
        /// </summary>
        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(savePath, json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"DesktopPetProgressService: failed to save progress. {exception.Message}");
            }
        }

        /// <summary>
        /// Gets the current amount of an inventory item.
        /// </summary>
        /// <param name="itemId">The item to query.</param>
        /// <returns>The stored amount, or zero when the item has not been collected.</returns>
        public int GetItemAmount(InventoryItemId itemId)
        {
            InventoryStack stack = FindStack(itemId);
            return stack != null ? stack.amount : 0;
        }

        /// <summary>
        /// Adds harvested crops to the crop inventory and unlocks the related catalog entry.
        /// </summary>
        /// <param name="cropId">The crop to add.</param>
        /// <param name="amount">The amount to add.</param>
        public void AddCrop(CropId cropId, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            InventoryItemId itemId = FarmDatabase.GetHarvestItem(cropId);
            InventoryStack stack = FindStack(itemId);
            if (stack == null)
            {
                stack = new InventoryStack { itemId = itemId };
                Data.inventory.Add(stack);
            }

            stack.amount += amount;
            UnlockCatalogEntry(CatalogDatabase.GetCropEntryId(cropId));
        }

        /// <summary>
        /// Gets the current amount of a cooked dish.
        /// </summary>
        /// <param name="recipeId">The dish recipe to query.</param>
        /// <returns>The stored amount, or zero when the dish has not been cooked.</returns>
        public int GetDishAmount(RecipeId recipeId)
        {
            DishInventoryStack stack = FindDishStack(recipeId);
            return stack != null ? stack.amount : 0;
        }

        /// <summary>
        /// Adds a cooked dish to dish storage and unlocks the related catalog entry.
        /// </summary>
        /// <param name="recipeId">The cooked recipe to add.</param>
        /// <param name="amount">The amount to add.</param>
        public void AddDish(RecipeId recipeId, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            DishInventoryStack stack = FindDishStack(recipeId);
            if (stack == null)
            {
                stack = new DishInventoryStack { recipeId = recipeId };
                Data.dishInventory.Add(stack);
            }

            stack.amount += amount;
            UnlockCatalogEntry(CatalogDatabase.GetRecipeEntryId(recipeId));
        }

        /// <summary>
        /// Attempts to consume cooked dishes from dish storage.
        /// </summary>
        /// <param name="recipeId">The dish recipe to consume.</param>
        /// <param name="amount">The amount to consume.</param>
        /// <returns><see langword="true"/> if enough dishes existed and were consumed; otherwise, <see langword="false"/>.</returns>
        public bool TryConsumeDish(RecipeId recipeId, int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            DishInventoryStack stack = FindDishStack(recipeId);
            if (stack == null || stack.amount < amount)
            {
                return false;
            }

            stack.amount -= amount;
            return true;
        }

        /// <summary>
        /// Determines whether the inventory contains all ingredients required by a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to inspect.</param>
        /// <returns><see langword="true"/> if all ingredients are available; otherwise, <see langword="false"/>.</returns>
        public bool HasIngredients(RecipeDefinition recipe)
        {
            foreach (IngredientRequirement ingredient in recipe.ingredients)
            {
                if (GetItemAmount(ingredient.itemId) < ingredient.amount)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to consume all ingredients required by a recipe.
        /// </summary>
        /// <param name="recipe">The recipe whose ingredients should be consumed.</param>
        /// <returns><see langword="true"/> if ingredients were consumed; otherwise, <see langword="false"/>.</returns>
        public bool TryConsumeIngredients(RecipeDefinition recipe)
        {
            if (!HasIngredients(recipe))
            {
                return false;
            }

            foreach (IngredientRequirement ingredient in recipe.ingredients)
            {
                InventoryStack stack = FindStack(ingredient.itemId);
                stack.amount -= ingredient.amount;
            }

            return true;
        }

        /// <summary>
        /// Attempts to start cooking through the shared progress service.
        /// </summary>
        /// <param name="recipeId">The recipe to cook.</param>
        /// <returns><see langword="true"/> if the job was added; otherwise, <see langword="false"/>.</returns>
        public bool TryStartCooking(RecipeId recipeId)
        {
            RecipeDefinition recipe = KitchenDatabase.GetRecipe(recipeId);
            if (!KitchenDatabase.IsRecipeUnlocked(recipeId, Data.FarmLevel) || !TryConsumeIngredients(recipe))
            {
                return false;
            }

            Data.cookingJobs.Add(new CookingJobState
            {
                recipeId = recipeId,
                startedAtUtc = DateTime.UtcNow.ToString("o"),
                cookMinutes = recipe.cookMinutes
            });

            return true;
        }

        /// <summary>
        /// Marks a cooking job as complete and stores the produced dish.
        /// </summary>
        /// <param name="job">The cooking job to complete.</param>
        public void CompleteCooking(CookingJobState job)
        {
            if (job == null || job.completed)
            {
                return;
            }

            RecipeDefinition recipe = KitchenDatabase.GetRecipe(job.recipeId);
            job.completed = true;
            Data.kitchenExperience += recipe.kitchenExperience;
            AddDish(recipe.id, 1);
        }

        /// <summary>
        /// Unlocks a catalog entry if it has not already been unlocked.
        /// </summary>
        /// <param name="entryId">The catalog entry identifier.</param>
        /// <returns><see langword="true"/> if a new entry was unlocked; otherwise, <see langword="false"/>.</returns>
        public bool UnlockCatalogEntry(string entryId)
        {
            if (Data.unlockedCatalogEntryIds.Contains(entryId))
            {
                return false;
            }

            Data.unlockedCatalogEntryIds.Add(entryId);
            return true;
        }

        /// <summary>
        /// Determines whether a catalog entry has been unlocked.
        /// </summary>
        /// <param name="entryId">The catalog entry identifier.</param>
        /// <returns><see langword="true"/> if the entry is unlocked; otherwise, <see langword="false"/>.</returns>
        public bool IsCatalogEntryUnlocked(string entryId)
        {
            return Data.unlockedCatalogEntryIds.Contains(entryId);
        }

        /// <summary>
        /// Gets the catalog completion ratio from zero to one.
        /// </summary>
        /// <returns>The completion ratio.</returns>
        public float GetCatalogCompletionRatio()
        {
            return CatalogDatabase.GetCompletionRatio(Data.unlockedCatalogEntryIds);
        }

        private InventoryStack FindStack(InventoryItemId itemId)
        {
            foreach (InventoryStack stack in Data.inventory)
            {
                if (stack.itemId == itemId)
                {
                    return stack;
                }
            }

            return null;
        }

        private DishInventoryStack FindDishStack(RecipeId recipeId)
        {
            foreach (DishInventoryStack stack in Data.dishInventory)
            {
                if (stack.recipeId == recipeId)
                {
                    return stack;
                }
            }

            return null;
        }

        private static DesktopPetProgressData CreateDefaultData()
        {
            DesktopPetProgressData data = new();
            data.EnsurePlotCount();
            return data;
        }
    }
}
