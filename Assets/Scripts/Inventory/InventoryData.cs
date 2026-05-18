using System;
using System.Collections.Generic;

namespace DesktopPet.Inventory
{
    /// <summary>
    /// Identifies inventory items produced by the farm and consumed by recipes.
    /// </summary>
    public enum InventoryItemId
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
    /// Defines display metadata for an inventory item.
    /// </summary>
    [Serializable]
    public class InventoryItemDefinition
    {
        public InventoryItemId id;
        public string displayName;
    }

    /// <summary>
    /// Provides static lookup data for inventory item display metadata.
    /// </summary>
    public static class InventoryDatabase
    {
        private static readonly InventoryItemDefinition[] items =
        {
            Item(InventoryItemId.Tomato, "番茄"),
            Item(InventoryItemId.Potato, "土豆"),
            Item(InventoryItemId.Rice, "水稻"),
            Item(InventoryItemId.Strawberry, "草莓"),
            Item(InventoryItemId.Corn, "玉米"),
            Item(InventoryItemId.Pumpkin, "南瓜"),
            Item(InventoryItemId.Wheat, "小麦"),
            Item(InventoryItemId.Blueberry, "蓝莓"),
            Item(InventoryItemId.Grape, "葡萄")
        };

        /// <summary>
        /// Gets all inventory item definitions.
        /// </summary>
        public static IReadOnlyList<InventoryItemDefinition> Items => items;

        /// <summary>
        /// Gets the display name for an inventory item.
        /// </summary>
        /// <param name="id">The item identifier.</param>
        /// <returns>The display name, or the enum name when no definition exists.</returns>
        public static string GetDisplayName(InventoryItemId id)
        {
            foreach (InventoryItemDefinition item in items)
            {
                if (item.id == id)
                {
                    return item.displayName;
                }
            }

            return id.ToString();
        }

        private static InventoryItemDefinition Item(InventoryItemId id, string displayName)
        {
            return new InventoryItemDefinition
            {
                id = id,
                displayName = displayName
            };
        }
    }
}
