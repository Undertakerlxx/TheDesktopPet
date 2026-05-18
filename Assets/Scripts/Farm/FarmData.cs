using System;
using System.Collections.Generic;
using DesktopPet.Inventory;

namespace DesktopPet.Farm
{
    /// <summary>
    /// Identifies the crop types supported by the farm module.
    /// </summary>
    public enum CropId
    {
        Tomato,
        Potato,
        Rice,
        Strawberry,
        Corn,
        Pumpkin,
        Wheat,
        Blueberry,
        Grape
    }

    /// <summary>
    /// Identifies the broad category of a crop.
    /// </summary>
    public enum CropCategory
    {
        Staple,
        Vegetable,
        Fruit
    }

    /// <summary>
    /// Defines static balance and display data for a crop.
    /// </summary>
    [Serializable]
    public class CropDefinition
    {
        public CropId id;
        public string displayName;
        public InventoryItemId outputItemId;
        public CropCategory category;
        public int unlockLevel;
        public int matureMinutes;
        public int yieldAmount;
        public int harvestExperience;
        public bool canFertilize;
        public string description;
    }

    /// <summary>
    /// Defines farm level thresholds, plot counts, and crop unlocks.
    /// </summary>
    [Serializable]
    public class FarmLevelDefinition
    {
        public int level;
        public int cumulativeExperience;
        public int plotCount;
        public CropId[] unlockedCrops;
    }

    /// <summary>
    /// Provides static lookup data and progression helpers for the farm module.
    /// </summary>
    public static class FarmDatabase
    {
        public const int MaxLevel = 8;
        public const int InitialPlotCount = 4;

        private static readonly CropDefinition[] crops =
        {
            new()
            {
                id = CropId.Tomato,
                displayName = "番茄",
                outputItemId = InventoryItemId.Tomato,
                category = CropCategory.Vegetable,
                unlockLevel = 1,
                matureMinutes = 180,
                yieldAmount = 2,
                harvestExperience = 5,
                canFertilize = true,
                description = "前期高频主力蔬菜，适合制作番茄汤和番茄炒饭。"
            },
            new()
            {
                id = CropId.Potato,
                displayName = "土豆",
                outputItemId = InventoryItemId.Potato,
                category = CropCategory.Vegetable,
                unlockLevel = 1,
                matureMinutes = 360,
                yieldAmount = 2,
                harvestExperience = 6,
                canFertilize = true,
                description = "前期稳定补给，可用于烤土豆和土豆面饼。"
            },
            new()
            {
                id = CropId.Rice,
                displayName = "水稻",
                outputItemId = InventoryItemId.Rice,
                category = CropCategory.Staple,
                unlockLevel = 1,
                matureMinutes = 480,
                yieldAmount = 2,
                harvestExperience = 6,
                canFertilize = true,
                description = "主食基础材料，收获后作为米饭系菜品材料使用。"
            },
            new()
            {
                id = CropId.Strawberry,
                displayName = "草莓",
                outputItemId = InventoryItemId.Strawberry,
                category = CropCategory.Fruit,
                unlockLevel = 2,
                matureMinutes = 240,
                yieldAmount = 2,
                harvestExperience = 5,
                canFertilize = true,
                description = "前期甜品材料，可制作草莓冰淇淋和草莓蛋糕。"
            },
            new()
            {
                id = CropId.Corn,
                displayName = "玉米",
                outputItemId = InventoryItemId.Corn,
                category = CropCategory.Staple,
                unlockLevel = 3,
                matureMinutes = 240,
                yieldAmount = 2,
                harvestExperience = 5,
                canFertilize = true,
                description = "过渡型高频主食作物，可制作爆米花和玉米炒饭。"
            },
            new()
            {
                id = CropId.Pumpkin,
                displayName = "南瓜",
                outputItemId = InventoryItemId.Pumpkin,
                category = CropCategory.Vegetable,
                unlockLevel = 4,
                matureMinutes = 480,
                yieldAmount = 2,
                harvestExperience = 6,
                canFertilize = true,
                description = "中期稳产蔬菜，可制作南瓜暖煲和南瓜粥。"
            },
            new()
            {
                id = CropId.Wheat,
                displayName = "小麦",
                outputItemId = InventoryItemId.Wheat,
                category = CropCategory.Staple,
                unlockLevel = 5,
                matureMinutes = 600,
                yieldAmount = 2,
                harvestExperience = 7,
                canFertilize = true,
                description = "面粉系核心材料，支撑面饼、面包和甜点线。"
            },
            new()
            {
                id = CropId.Blueberry,
                displayName = "蓝莓",
                outputItemId = InventoryItemId.Blueberry,
                category = CropCategory.Fruit,
                unlockLevel = 6,
                matureMinutes = 720,
                yieldAmount = 2,
                harvestExperience = 7,
                canFertilize = true,
                description = "后期甜品材料，可制作水果拼盘和蓝莓派。"
            },
            new()
            {
                id = CropId.Grape,
                displayName = "葡萄",
                outputItemId = InventoryItemId.Grape,
                category = CropCategory.Fruit,
                unlockLevel = 7,
                matureMinutes = 720,
                yieldAmount = 2,
                harvestExperience = 7,
                canFertilize = true,
                description = "后期饮品和甜点材料，可制作葡萄果饮和葡萄挞。"
            }
        };

        private static readonly FarmLevelDefinition[] levels =
        {
            new() { level = 1, cumulativeExperience = 0, plotCount = 4, unlockedCrops = new[] { CropId.Tomato, CropId.Potato, CropId.Rice } },
            new() { level = 2, cumulativeExperience = 30, plotCount = 5, unlockedCrops = new[] { CropId.Strawberry } },
            new() { level = 3, cumulativeExperience = 80, plotCount = 5, unlockedCrops = new[] { CropId.Corn } },
            new() { level = 4, cumulativeExperience = 160, plotCount = 6, unlockedCrops = new[] { CropId.Pumpkin } },
            new() { level = 5, cumulativeExperience = 280, plotCount = 6, unlockedCrops = new[] { CropId.Wheat } },
            new() { level = 6, cumulativeExperience = 450, plotCount = 7, unlockedCrops = new[] { CropId.Blueberry } },
            new() { level = 7, cumulativeExperience = 680, plotCount = 7, unlockedCrops = new[] { CropId.Grape } },
            new() { level = 8, cumulativeExperience = 1000, plotCount = 8, unlockedCrops = Array.Empty<CropId>() }
        };

        public static IReadOnlyList<CropDefinition> Crops => crops;
        public static IReadOnlyList<FarmLevelDefinition> Levels => levels;

        public static CropDefinition GetCrop(CropId id)
        {
            foreach (CropDefinition crop in crops)
            {
                if (crop.id == id)
                {
                    return crop;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown crop id.");
        }

        public static int GetLevelForExperience(int experience)
        {
            int level = 1;
            foreach (FarmLevelDefinition definition in levels)
            {
                if (experience >= definition.cumulativeExperience)
                {
                    level = definition.level;
                }
            }

            return level;
        }

        public static int GetCumulativeExperienceForLevel(int level)
        {
            int cumulativeExperience = 0;
            foreach (FarmLevelDefinition definition in levels)
            {
                if (level >= definition.level)
                {
                    cumulativeExperience = definition.cumulativeExperience;
                }
            }

            return cumulativeExperience;
        }

        public static int GetExperienceInCurrentLevel(int experience)
        {
            int currentLevel = GetLevelForExperience(experience);
            return Math.Max(0, experience - GetCumulativeExperienceForLevel(currentLevel));
        }

        public static int GetExperienceToNextLevel(int level)
        {
            int currentLevelExperience = GetCumulativeExperienceForLevel(level);
            foreach (FarmLevelDefinition definition in levels)
            {
                if (definition.level == level + 1)
                {
                    return Math.Max(0, definition.cumulativeExperience - currentLevelExperience);
                }
            }

            return 0;
        }

        public static int GetPlotCountForLevel(int level)
        {
            int plotCount = InitialPlotCount;
            foreach (FarmLevelDefinition definition in levels)
            {
                if (level >= definition.level)
                {
                    plotCount = definition.plotCount;
                }
            }

            return plotCount;
        }

        public static bool IsCropUnlocked(CropId cropId, int farmLevel)
        {
            return farmLevel >= GetCrop(cropId).unlockLevel;
        }

        public static InventoryItemId GetHarvestItem(CropId cropId)
        {
            return GetCrop(cropId).outputItemId;
        }
    }
}
