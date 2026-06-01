using System;
using System.IO;
using DesktopPet.MiniGame;
using UnityEngine;

namespace DesktopPet.Accounts
{
    public class AccountMigrationService
    {
        private const string LegacyPetStatsPath = "Save/the-pet-stats.json";
        private const string LegacyProgressPath = "farm-kitchen-catalog-progress.json";
        private const string LegacyTimerHistoryPath = "timer-history.json";
        private const string LegacyAchievementPath = "achievement-progress.json";

        private readonly AccountDatabase accountDatabase;

        public AccountMigrationService(AccountDatabase accountDatabase = null)
        {
            this.accountDatabase = accountDatabase ?? new AccountDatabase();
        }

        public bool HasPendingLegacyData()
        {
            MigrationStateData state = LoadState();
            return state.legacyDataStaged;
        }

        public bool TryStageLegacyData()
        {
            AccountListData accounts = accountDatabase.LoadAccounts();
            if (accounts.accounts.Count > 0)
            {
                return false;
            }

            MigrationStateData state = LoadState();
            if (state.legacyDataStaged || !HasAnyLegacyData())
            {
                return false;
            }

            try
            {
                if (Directory.Exists(AccountPathProvider.LegacyImportRootPath))
                {
                    Directory.Delete(AccountPathProvider.LegacyImportRootPath, true);
                }

                Directory.CreateDirectory(AccountPathProvider.LegacyImportRootPath);
                CopyLegacyFileIfExists(GetLegacyAbsolutePath(LegacyPetStatsPath), Path.Combine(AccountPathProvider.LegacyImportRootPath, "pet-stats.json"));
                CopyLegacyFileIfExists(GetLegacyAbsolutePath(LegacyProgressPath), Path.Combine(AccountPathProvider.LegacyImportRootPath, "progress.json"));
                CopyLegacyFileIfExists(GetLegacyAbsolutePath(LegacyTimerHistoryPath), Path.Combine(AccountPathProvider.LegacyImportRootPath, "timer-history.json"));
                CopyLegacyFileIfExists(GetLegacyAbsolutePath(LegacyAchievementPath), Path.Combine(AccountPathProvider.LegacyImportRootPath, "achievement-progress.json"));

                if (MiniGameRecordStorage.HasLegacyPlayerPrefsData())
                {
                    MiniGameRecordsData legacyRecords = MiniGameRecordStorage.LoadLegacyPlayerPrefs();
                    MiniGameRecordStorage.SaveToPath(Path.Combine(AccountPathProvider.LegacyImportRootPath, "mini-game-records.json"), legacyRecords);
                }

                state.legacyDataStaged = true;
                SaveState(state);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"AccountMigrationService: failed to stage legacy data. {exception.Message}");
                return false;
            }
        }

        public bool TryImportPendingLegacyData(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                return false;
            }

            MigrationStateData state = LoadState();
            if (!state.legacyDataStaged || !Directory.Exists(AccountPathProvider.LegacyImportRootPath))
            {
                return false;
            }

            try
            {
                string accountRoot = AccountPathProvider.GetAccountRoot(accountId);
                Directory.CreateDirectory(accountRoot);

                foreach (string stagedFile in Directory.GetFiles(AccountPathProvider.LegacyImportRootPath))
                {
                    string destinationPath = Path.Combine(accountRoot, Path.GetFileName(stagedFile));
                    File.Copy(stagedFile, destinationPath, true);
                }

                Directory.Delete(AccountPathProvider.LegacyImportRootPath, true);
                state.legacyDataStaged = false;
                state.importedAccountId = accountId;
                SaveState(state);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"AccountMigrationService: failed to import legacy data. {exception.Message}");
                return false;
            }
        }

        private static bool HasAnyLegacyData()
        {
            return File.Exists(GetLegacyAbsolutePath(LegacyPetStatsPath)) ||
                   File.Exists(GetLegacyAbsolutePath(LegacyProgressPath)) ||
                   File.Exists(GetLegacyAbsolutePath(LegacyTimerHistoryPath)) ||
                   File.Exists(GetLegacyAbsolutePath(LegacyAchievementPath)) ||
                   MiniGameRecordStorage.HasLegacyPlayerPrefsData();
        }

        private static string GetLegacyAbsolutePath(string relativePath)
        {
            return Path.Combine(Application.persistentDataPath, relativePath);
        }

        private static void CopyLegacyFileIfExists(string sourcePath, string destinationPath)
        {
            if (!File.Exists(sourcePath))
            {
                return;
            }

            File.Copy(sourcePath, destinationPath, true);
        }

        private static MigrationStateData LoadState()
        {
            if (!File.Exists(AccountPathProvider.MigrationFilePath))
            {
                return new MigrationStateData();
            }

            try
            {
                string json = File.ReadAllText(AccountPathProvider.MigrationFilePath);
                return JsonUtility.FromJson<MigrationStateData>(json) ?? new MigrationStateData();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"AccountMigrationService: failed to load migration state. {exception.Message}");
                return new MigrationStateData();
            }
        }

        private static void SaveState(MigrationStateData state)
        {
            state ??= new MigrationStateData();

            try
            {
                Directory.CreateDirectory(AccountPathProvider.AccountsRootPath);
                string json = JsonUtility.ToJson(state, true);
                File.WriteAllText(AccountPathProvider.MigrationFilePath, json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"AccountMigrationService: failed to save migration state. {exception.Message}");
            }
        }
    }
}
