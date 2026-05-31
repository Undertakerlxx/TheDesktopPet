using System;
using DesktopPet.Achievements;
using DesktopPet.Catalog;
using DesktopPet.Progress;

namespace DesktopPet.Kitchen
{
    /// <summary>
    /// Provides kitchen gameplay operations such as recipe checks, cooking jobs, and dish rewards.
    /// </summary>
    public class KitchenService
    {
        private const bool UseFiveSecondCookingForTesting = true;
        private static readonly TimeSpan TestCookingDuration = TimeSpan.FromSeconds(5);

        private readonly DesktopPetProgressService progressService;

        /// <summary>
        /// Gets the shared progress data used by the kitchen module.
        /// </summary>
        public DesktopPetProgressData Progress => progressService.Data;

        /// <summary>
        /// Gets the current farm level used to unlock recipes.
        /// </summary>
        public int FarmLevel => Progress.FarmLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="KitchenService"/> class.
        /// </summary>
        /// <param name="progressService">The progress service that owns inventory and cooking data.</param>
        public KitchenService(DesktopPetProgressService progressService)
        {
            this.progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            RemoveCompletedJobs();
        }

        /// <summary>
        /// Determines whether a recipe is unlocked for the current farm level.
        /// </summary>
        /// <param name="recipeId">The recipe to inspect.</param>
        /// <returns><see langword="true"/> if the recipe is unlocked; otherwise, <see langword="false"/>.</returns>
        public bool IsUnlocked(RecipeId recipeId)
        {
            return KitchenDatabase.IsRecipeUnlocked(recipeId, FarmLevel);
        }

        /// <summary>
        /// Determines whether all ingredients required by a recipe are available.
        /// </summary>
        /// <param name="recipe">The recipe to inspect.</param>
        /// <returns><see langword="true"/> if all ingredients are available; otherwise, <see langword="false"/>.</returns>
        public bool HasIngredients(RecipeDefinition recipe)
        {
            return progressService.HasIngredients(recipe);
        }

        /// <summary>
        /// Determines whether a cooking job can be started for the specified recipe.
        /// </summary>
        /// <param name="recipeId">The recipe to cook.</param>
        /// <returns><see langword="true"/> if cooking can start; otherwise, <see langword="false"/>.</returns>
        public bool CanStartCooking(RecipeId recipeId)
        {
            RecipeDefinition recipe = KitchenDatabase.GetRecipe(recipeId);
            return IsUnlocked(recipeId) && HasIngredients(recipe);
        }

        /// <summary>
        /// Attempts to consume ingredients and create a cooking job.
        /// </summary>
        /// <param name="recipeId">The recipe to cook.</param>
        /// <returns><see langword="true"/> if the job was created; otherwise, <see langword="false"/>.</returns>
        public bool TryStartCooking(RecipeId recipeId)
        {
            if (!CanStartCooking(recipeId))
            {
                return false;
            }

            RecipeDefinition recipe = KitchenDatabase.GetRecipe(recipeId);
            progressService.TryConsumeIngredients(recipe);
            Progress.cookingJobs.Add(new CookingJobState
            {
                recipeId = recipeId,
                startedAtUtc = DateTime.UtcNow.ToString("o"),
                cookMinutes = recipe.cookMinutes
            });

            progressService.Save();
            return true;
        }

        /// <summary>
        /// Gets the cooking duration currently used by the kitchen.
        /// </summary>
        /// <param name="recipe">The recipe whose duration should be calculated.</param>
        /// <returns>The active cooking duration. In test mode this is five seconds.</returns>
        public TimeSpan GetCookDuration(RecipeDefinition recipe)
        {
            return UseFiveSecondCookingForTesting
                ? TestCookingDuration
                : TimeSpan.FromMinutes(recipe.cookMinutes);
        }

        /// <summary>
        /// Determines whether a cooking job can be completed.
        /// </summary>
        /// <param name="job">The cooking job to inspect.</param>
        /// <returns><see langword="true"/> if the job is complete and claimable; otherwise, <see langword="false"/>.</returns>
        public bool CanComplete(CookingJobState job)
        {
            return job != null && !job.completed && GetRemainingTime(job) == TimeSpan.Zero;
        }

        /// <summary>
        /// Attempts to complete a cooking job, add the dish to storage, and remove the job from the queue.
        /// </summary>
        /// <param name="job">The cooking job to complete.</param>
        /// <returns><see langword="true"/> if completion succeeded; otherwise, <see langword="false"/>.</returns>
        public bool TryComplete(CookingJobState job)
        {
            if (!CanComplete(job))
            {
                return false;
            }

            RecipeDefinition recipe = KitchenDatabase.GetRecipe(job.recipeId);
            Progress.kitchenExperience += recipe.kitchenExperience;
            progressService.AddDish(recipe.id, 1);
            Progress.cookingJobs.Remove(job);
            progressService.Save();
            AchievementEventRecorder.Record(AchievementEventType.KitchenCook);
            return true;
        }

        /// <summary>
        /// Gets the remaining time for a cooking job.
        /// </summary>
        /// <param name="job">The cooking job to inspect.</param>
        /// <returns>The remaining time, or <see cref="TimeSpan.Zero"/> when the job can be completed.</returns>
        public TimeSpan GetRemainingTime(CookingJobState job)
        {
            if (job == null || job.completed)
            {
                return TimeSpan.Zero;
            }

            DateTime startedAt = ParseUtcTime(job.startedAtUtc);
            RecipeDefinition recipe = KitchenDatabase.GetRecipe(job.recipeId);
            DateTime completeAt = startedAt.Add(GetCookDuration(recipe));
            TimeSpan remaining = completeAt - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        private static DateTime ParseUtcTime(string value)
        {
            if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime result))
            {
                return result.ToUniversalTime();
            }

            return DateTime.UtcNow;
        }

        private void RemoveCompletedJobs()
        {
            int removedCount = Progress.cookingJobs.RemoveAll(job => job != null && job.completed);
            if (removedCount > 0)
            {
                progressService.Save();
            }
        }
    }
}
