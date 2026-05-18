using System;
using System.Collections.Generic;
using DesktopPet.Farm;
using DesktopPet.Kitchen;

namespace DesktopPet.Catalog
{
    /// <summary>
    /// Identifies the category of a catalog entry.
    /// </summary>
    public enum CatalogEntryType
    {
        Crop,
        Recipe
    }

    /// <summary>
    /// Defines a crop or recipe entry shown in the catalog.
    /// </summary>
    [Serializable]
    public class CatalogEntryDefinition
    {
        public string id;
        public CatalogEntryType type;
        public string displayName;
        public string unlockHint;
        public int firstUnlockIntimacyReward;
        public string description;
    }

    /// <summary>
    /// Provides static lookup data for crop and recipe catalog entries.
    /// </summary>
    public static class CatalogDatabase
    {
        private static readonly CatalogEntryDefinition[] entries = BuildEntries();

        /// <summary>
        /// Gets all catalog entries.
        /// </summary>
        public static IReadOnlyList<CatalogEntryDefinition> Entries => entries;

        /// <summary>
        /// Builds the catalog entry identifier for a crop.
        /// </summary>
        /// <param name="cropId">The crop identifier.</param>
        /// <returns>The catalog entry identifier.</returns>
        public static string GetCropEntryId(CropId cropId)
        {
            return $"crop:{cropId}";
        }

        /// <summary>
        /// Builds the catalog entry identifier for a recipe.
        /// </summary>
        /// <param name="recipeId">The recipe identifier.</param>
        /// <returns>The catalog entry identifier.</returns>
        public static string GetRecipeEntryId(RecipeId recipeId)
        {
            return $"recipe:{recipeId}";
        }

        /// <summary>
        /// Gets a catalog entry by identifier.
        /// </summary>
        /// <param name="id">The catalog entry identifier.</param>
        /// <returns>The matching catalog entry.</returns>
        /// <exception cref="ArgumentException">Thrown when the identifier is unknown.</exception>
        public static CatalogEntryDefinition GetEntry(string id)
        {
            foreach (CatalogEntryDefinition entry in entries)
            {
                if (entry.id == id)
                {
                    return entry;
                }
            }

            throw new ArgumentException($"Unknown catalog entry id: {id}", nameof(id));
        }

        /// <summary>
        /// Calculates the ratio of unlocked catalog entries.
        /// </summary>
        /// <param name="unlockedEntryIds">The unlocked catalog entry identifiers.</param>
        /// <returns>A value between zero and one.</returns>
        public static float GetCompletionRatio(ICollection<string> unlockedEntryIds)
        {
            if (entries.Length == 0)
            {
                return 1f;
            }

            int unlockedCount = 0;
            foreach (CatalogEntryDefinition entry in entries)
            {
                if (unlockedEntryIds.Contains(entry.id))
                {
                    unlockedCount++;
                }
            }

            return (float)unlockedCount / entries.Length;
        }

        private static CatalogEntryDefinition[] BuildEntries()
        {
            List<CatalogEntryDefinition> result = new();

            foreach (CropDefinition crop in FarmDatabase.Crops)
            {
                result.Add(new CatalogEntryDefinition
                {
                    id = GetCropEntryId(crop.id),
                    type = CatalogEntryType.Crop,
                    displayName = crop.displayName,
                    unlockHint = "首次收获某种作物",
                    firstUnlockIntimacyReward = 10,
                    description = crop.description
                });
            }

            foreach (RecipeDefinition recipe in KitchenDatabase.Recipes)
            {
                result.Add(new CatalogEntryDefinition
                {
                    id = GetRecipeEntryId(recipe.id),
                    type = CatalogEntryType.Recipe,
                    displayName = recipe.displayName,
                    unlockHint = "首次制作某道料理",
                    firstUnlockIntimacyReward = 15,
                    description = recipe.description
                });
            }

            return result.ToArray();
        }
    }
}
