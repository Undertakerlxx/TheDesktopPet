using System;
using DesktopPet.Achievements;
using DesktopPet.Progress;
using UnityEngine;

namespace DesktopPet.Farm
{
    /// <summary>
    /// Provides farm gameplay operations such as planting, growth checks, and harvesting.
    /// </summary>
    public class FarmService
    {
        private static readonly bool UseFiveSecondGrowthForTesting = true;
        private static readonly TimeSpan TestGrowthDuration = TimeSpan.FromSeconds(5);

        private readonly DesktopPetProgressService progressService;
        private ThePetStatsManager statsManager;

        /// <summary>
        /// Gets the shared progress data used by the farm module.
        /// </summary>
        public DesktopPetProgressData Progress => progressService.Data;

        /// <summary>
        /// Gets the current farm level derived from accumulated farm experience.
        /// </summary>
        public int FarmLevel => Progress.FarmLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="FarmService"/> class.
        /// </summary>
        /// <param name="progressService">The progress service that owns persisted farm data.</param>
        public FarmService(DesktopPetProgressService progressService)
        {
            this.progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            statsManager = UnityEngine.Object.FindFirstObjectByType<ThePetStatsManager>();
            Progress.EnsurePlotCount();
        }

        /// <summary>
        /// Determines whether a crop can be planted in the specified plot.
        /// </summary>
        /// <param name="plotIndex">The zero-based plot index.</param>
        /// <param name="cropId">The crop to plant.</param>
        /// <returns><see langword="true"/> if the crop can be planted; otherwise, <see langword="false"/>.</returns>
        public bool CanPlant(int plotIndex, CropId cropId)
        {
            if (!FarmDatabase.IsCropUnlocked(cropId, FarmLevel))
            {
                return false;
            }

            FarmPlotState plot = GetPlot(plotIndex);
            return plot != null && !plot.isPlanted;
        }

        /// <summary>
        /// Attempts to plant a crop in the specified plot and saves progress on success.
        /// </summary>
        /// <param name="plotIndex">The zero-based plot index.</param>
        /// <param name="cropId">The crop to plant.</param>
        /// <returns><see langword="true"/> if planting succeeded; otherwise, <see langword="false"/>.</returns>
        public bool TryPlant(int plotIndex, CropId cropId)
        {
            if (!CanPlant(plotIndex, cropId))
            {
                return false;
            }

            CropDefinition crop = FarmDatabase.GetCrop(cropId);
            FarmPlotState plot = GetPlot(plotIndex);
            plot.isPlanted = true;
            plot.cropId = cropId;
            plot.plantedAtUtc = DateTime.UtcNow.ToString("o");
            plot.matureMinutes = GetEffectiveDurationMinutes(crop.matureMinutes);
            plot.fertilized = false;

            Progress.farmExperience += 1;
            Progress.EnsurePlotCount();
            progressService.Save();
            return true;
        }

        /// <summary>
        /// Determines whether the specified plot is ready to harvest.
        /// </summary>
        /// <param name="plotIndex">The zero-based plot index.</param>
        /// <returns><see langword="true"/> if the plot can be harvested; otherwise, <see langword="false"/>.</returns>
        public bool CanHarvest(int plotIndex)
        {
            FarmPlotState plot = GetPlot(plotIndex);
            return plot != null && plot.isPlanted && IsMature(plot);
        }

        /// <summary>
        /// Attempts to harvest a mature crop, add it to storage, and clear the plot.
        /// </summary>
        /// <param name="plotIndex">The zero-based plot index.</param>
        /// <param name="harvestedCropId">When this method returns, contains the harvested crop identifier.</param>
        /// <param name="amount">When this method returns, contains the harvested amount.</param>
        /// <returns><see langword="true"/> if harvest succeeded; otherwise, <see langword="false"/>.</returns>
        public bool TryHarvest(int plotIndex, out CropId harvestedCropId, out int amount)
        {
            harvestedCropId = default;
            amount = 0;

            if (!CanHarvest(plotIndex))
            {
                return false;
            }

            FarmPlotState plot = GetPlot(plotIndex);
            CropDefinition crop = FarmDatabase.GetCrop(plot.cropId);
            harvestedCropId = crop.id;
            amount = crop.yieldAmount;

            progressService.AddCrop(crop.id, crop.yieldAmount);
            Progress.farmExperience += crop.harvestExperience;
            ClearPlot(plot);
            Progress.EnsurePlotCount();
            progressService.Save();
            AchievementEventRecorder.Record(AchievementEventType.FarmHarvest);
            return true;
        }

        /// <summary>
        /// Gets the remaining growth time for a planted plot.
        /// </summary>
        /// <param name="plot">The plot state to inspect.</param>
        /// <returns>The remaining time, or <see cref="TimeSpan.Zero"/> when the crop is mature.</returns>
        public TimeSpan GetRemainingTime(FarmPlotState plot)
        {
            if (plot == null || !plot.isPlanted)
            {
                return TimeSpan.Zero;
            }

            DateTime plantedAt = ParseUtcTime(plot.plantedAtUtc);
            TimeSpan growthDuration = GetGrowthDuration(plot);
            DateTime matureAt = plantedAt.Add(growthDuration);
            TimeSpan remaining = matureAt - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        /// <summary>
        /// Determines whether the specified plot has finished growing.
        /// </summary>
        /// <param name="plot">The plot state to inspect.</param>
        /// <returns><see langword="true"/> when the plot is mature; otherwise, <see langword="false"/>.</returns>
        public bool IsMature(FarmPlotState plot)
        {
            return GetRemainingTime(plot) == TimeSpan.Zero;
        }

        /// <summary>
        /// Gets a plot by index, expanding default plot data when needed.
        /// </summary>
        /// <param name="plotIndex">The zero-based plot index.</param>
        /// <returns>The plot state, or <see langword="null"/> when the index is outside the saved plot list.</returns>
        public FarmPlotState GetPlot(int plotIndex)
        {
            Progress.EnsurePlotCount();
            if (plotIndex < 0 || plotIndex >= Progress.farmPlots.Count)
            {
                return null;
            }

            return Progress.farmPlots[plotIndex];
        }

        private static void ClearPlot(FarmPlotState plot)
        {
            plot.isPlanted = false;
            plot.cropId = default;
            plot.plantedAtUtc = string.Empty;
            plot.matureMinutes = 0;
            plot.fertilized = false;
        }

        private static DateTime ParseUtcTime(string value)
        {
            if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime result))
            {
                return result.ToUniversalTime();
            }

            return DateTime.UtcNow;
        }

        private int GetEffectiveDurationMinutes(int baseMinutes)
        {
            return ProgressionDatabase.ApplyFocusEfficiency(baseMinutes, GetCurrentFocusValue());
        }

        private float GetCurrentFocusValue()
        {
            statsManager ??= UnityEngine.Object.FindFirstObjectByType<ThePetStatsManager>();
            return statsManager != null && statsManager.current_stats != null
                ? statsManager.current_stats.focus
                : 0f;
        }

        private static TimeSpan GetGrowthDuration(FarmPlotState plot)
        {
            if (!UseFiveSecondGrowthForTesting)
            {
                return TimeSpan.FromMinutes(Math.Max(1, plot.matureMinutes));
            }

            CropDefinition crop = FarmDatabase.GetCrop(plot.cropId);
            int baseMinutes = Math.Max(1, crop.matureMinutes);
            int effectiveMinutes = Math.Max(1, plot.matureMinutes);
            double ratio = effectiveMinutes / (double)baseMinutes;
            double seconds = Math.Max(1d, Math.Ceiling(TestGrowthDuration.TotalSeconds * ratio));
            return TimeSpan.FromSeconds(seconds);
        }
    }
}
