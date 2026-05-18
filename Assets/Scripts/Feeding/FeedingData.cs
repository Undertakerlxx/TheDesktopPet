using System;
using DesktopPet.Kitchen;

namespace DesktopPet.Feeding
{
    /// <summary>
    /// Identifies the broad dish category requested by the pet.
    /// </summary>
    public enum DishCategory
    {
        Staple,
        Soup,
        Dessert,
        Drink,
        Snack,
        VegetableDish
    }

    /// <summary>
    /// Stores the current pet food preference.
    /// </summary>
    [Serializable]
    public class FeedingRequestState
    {
        public DishCategory requestedCategory;
        public string createdAtUtc;
    }

    /// <summary>
    /// Describes the result of a feeding attempt.
    /// </summary>
    public class FeedingResult
    {
        public bool success;
        public bool matchedPreference;
        public RecipeId recipeId;
        public DishCategory previousCategory;
        public DishCategory currentCategory;
        public string message;
    }
}
