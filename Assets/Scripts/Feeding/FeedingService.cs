using System;
using DesktopPet.Kitchen;
using DesktopPet.Progress;
using UnityEngine;

namespace DesktopPet.Feeding
{
    /// <summary>
    /// Handles pet feeding rules without directly depending on farm, kitchen, or UI code.
    /// </summary>
    public class FeedingService
    {
        private readonly DesktopPetProgressService progressService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedingService"/> class.
        /// </summary>
        /// <param name="progressService">The shared progress service used to read and write dish inventory.</param>
        public FeedingService(DesktopPetProgressService progressService)
        {
            this.progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            EnsureRequest();
        }

        /// <summary>
        /// Gets the current feeding request, creating one when no request exists.
        /// </summary>
        /// <returns>The active feeding request state.</returns>
        public FeedingRequestState EnsureRequest()
        {
            progressService.Data.EnsureCollections();
            if (progressService.Data.feedingRequest == null)
            {
                RefreshRequest();
            }

            return progressService.Data.feedingRequest;
        }

        /// <summary>
        /// Determines whether a recipe matches the pet's current requested category.
        /// </summary>
        /// <param name="recipe">The recipe to inspect.</param>
        /// <returns><see langword="true"/> if the recipe satisfies the current request.</returns>
        public bool IsPreferred(RecipeDefinition recipe)
        {
            return recipe != null && recipe.category == EnsureRequest().requestedCategory;
        }

        /// <summary>
        /// Attempts to consume one stored dish and feed it to the pet.
        /// </summary>
        /// <param name="recipeId">The dish recipe to feed.</param>
        /// <param name="result">When this method returns, contains the feeding result details.</param>
        /// <returns><see langword="true"/> if one dish was consumed; otherwise, <see langword="false"/>.</returns>
        public bool TryFeed(RecipeId recipeId, out FeedingResult result)
        {
            RecipeDefinition recipe = KitchenDatabase.GetRecipe(recipeId);
            DishCategory previousCategory = EnsureRequest().requestedCategory;

            result = new FeedingResult
            {
                recipeId = recipeId,
                previousCategory = previousCategory,
                currentCategory = previousCategory
            };

            if (!progressService.TryConsumeDish(recipeId, 1))
            {
                result.success = false;
                result.message = "菜品不足";
                return false;
            }

            bool matchedPreference = recipe.category == previousCategory;
            result.success = true;
            result.matchedPreference = matchedPreference;

            if (matchedPreference)
            {
                RefreshRequest();
                result.currentCategory = progressService.Data.feedingRequest.requestedCategory;
                result.message = "正合胃口！";
            }
            else
            {
                result.message = "喂食成功";
            }

            progressService.Save();
            return true;
        }

        /// <summary>
        /// Refreshes the pet's requested food category.
        /// </summary>
        /// <returns>The newly generated request state.</returns>
        public FeedingRequestState RefreshRequest()
        {
            Array values = Enum.GetValues(typeof(DishCategory));
            DishCategory nextCategory = (DishCategory)values.GetValue(UnityEngine.Random.Range(0, values.Length));
            progressService.Data.feedingRequest = new FeedingRequestState
            {
                requestedCategory = nextCategory,
                createdAtUtc = DateTime.UtcNow.ToString("o")
            };

            progressService.Save();
            return progressService.Data.feedingRequest;
        }

        /// <summary>
        /// Gets a player-facing display name for a dish category.
        /// </summary>
        /// <param name="category">The category to display.</param>
        /// <returns>The localized category name.</returns>
        public static string GetCategoryDisplayName(DishCategory category)
        {
            return category switch
            {
                DishCategory.Staple => "主食",
                DishCategory.Soup => "汤类",
                DishCategory.Dessert => "甜品",
                DishCategory.Drink => "饮品",
                DishCategory.Snack => "零食",
                DishCategory.VegetableDish => "蔬菜料理",
                _ => category.ToString()
            };
        }
    }
}
