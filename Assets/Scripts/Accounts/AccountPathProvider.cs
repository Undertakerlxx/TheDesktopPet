using System;
using System.IO;
using UnityEngine;

namespace DesktopPet.Accounts
{
    public static class AccountPathProvider
    {
        private const string AccountsDirectoryName = "Accounts";
        private const string LegacyImportDirectoryName = "LegacyImport";
        private const string NoSessionDirectoryName = "_no-session";

        public static string AccountsRootPath => Path.Combine(Application.persistentDataPath, AccountsDirectoryName);

        public static string AccountsFilePath => Path.Combine(AccountsRootPath, "accounts.json");

        public static string CurrentAccountFilePath => Path.Combine(AccountsRootPath, "current-account.json");

        public static string MigrationFilePath => Path.Combine(AccountsRootPath, "migrations.json");

        public static string LegacyImportRootPath => Path.Combine(AccountsRootPath, LegacyImportDirectoryName);

        public static string GetAccountRoot(string accountId)
        {
            return Path.Combine(AccountsRootPath, string.IsNullOrWhiteSpace(accountId) ? NoSessionDirectoryName : accountId);
        }

        public static string GetCurrentAccountRoot()
        {
            return GetAccountRoot(AccountSession.CurrentAccountId);
        }

        public static string GetPetStatsPath()
        {
            return Path.Combine(GetCurrentAccountRoot(), "pet-stats.json");
        }

        public static string GetProgressPath()
        {
            return Path.Combine(GetCurrentAccountRoot(), "progress.json");
        }

        public static string GetTimerHistoryPath()
        {
            return Path.Combine(GetCurrentAccountRoot(), "timer-history.json");
        }

        public static string GetAchievementPath()
        {
            return Path.Combine(GetCurrentAccountRoot(), "achievement-progress.json");
        }

        public static string GetMiniGameRecordsPath()
        {
            return Path.Combine(GetCurrentAccountRoot(), "mini-game-records.json");
        }
    }
}
