using System;
using System.Collections.Generic;
using DesktopPet.Feeding;
using DesktopPet.Farm;
using DesktopPet.Inventory;
using DesktopPet.Kitchen;

namespace DesktopPet.Progress
{
    /// <summary>
    /// Represents a crop or material stack in player inventory.
    /// </summary>
    [Serializable]
    public class InventoryStack
    {
        public InventoryItemId itemId;
        public int amount;
    }

    /// <summary>
    /// Represents a cooked dish stack in dish storage.
    /// </summary>
    [Serializable]
    public class DishInventoryStack
    {
        public RecipeId recipeId;
        public int amount;
    }

    /// <summary>
    /// Represents the saved state of a farm plot.
    /// </summary>
    [Serializable]
    public class FarmPlotState
    {
        public int plotIndex;
        public bool isPlanted;
        public CropId cropId;
        public string plantedAtUtc;
        public int matureMinutes;
        public bool fertilized;
    }

    /// <summary>
    /// Represents an active or completed cooking job.
    /// </summary>
    [Serializable]
    public class CookingJobState
    {
        public RecipeId recipeId;
        public string startedAtUtc;
        public int cookMinutes;
        public bool completed;
    }

    /// <summary>
    /// Contains persistent data shared by farm, kitchen, storage, and catalog modules.
    /// </summary>
    [Serializable]
    public class DesktopPetProgressData
    {
        public int farmExperience;
        public int kitchenExperience;
        public List<InventoryStack> inventory = new();
        public List<DishInventoryStack> dishInventory = new();
        public List<FarmPlotState> farmPlots = new();
        public List<CookingJobState> cookingJobs = new();
        public List<string> unlockedCatalogEntryIds = new();
        public FeedingRequestState feedingRequest;

        /// <summary>
        /// Gets the current farm level derived from farm experience.
        /// </summary>
        public int FarmLevel => FarmDatabase.GetLevelForExperience(farmExperience);

        /// <summary>
        /// Ensures collection fields are initialized after loading older save files.
        /// </summary>
        public void EnsureCollections()
        {
            inventory ??= new List<InventoryStack>();
            dishInventory ??= new List<DishInventoryStack>();
            farmPlots ??= new List<FarmPlotState>();
            cookingJobs ??= new List<CookingJobState>();
            unlockedCatalogEntryIds ??= new List<string>();
        }

        /// <summary>
        /// Ensures the saved farm plot list contains enough plots for the current level.
        /// </summary>
        public void EnsurePlotCount()
        {
            EnsureCollections();
            int expectedCount = FarmDatabase.GetPlotCountForLevel(FarmLevel);
            for (int i = farmPlots.Count; i < expectedCount; i++)
            {
                farmPlots.Add(new FarmPlotState { plotIndex = i });
            }
        }
    }
}
