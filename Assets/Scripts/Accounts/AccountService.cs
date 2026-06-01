using System;
using System.Collections.Generic;
using System.IO;

namespace DesktopPet.Accounts
{
    public enum AccountLoginResult
    {
        Success,
        InvalidUsernameOrPassword
    }

    public enum AccountRegisterResult
    {
        Success,
        UsernameAlreadyExists,
        InvalidUsername,
        InvalidPassword
    }

    public class AccountService
    {
        private const int MinUsernameLength = 3;
        private const int MaxUsernameLength = 20;
        private const int MinPasswordLength = 6;
        private const int MaxPasswordLength = 32;

        private readonly AccountDatabase database;
        private readonly AccountMigrationService migrationService;

        public AccountService(AccountDatabase database = null, AccountMigrationService migrationService = null)
        {
            this.database = database ?? new AccountDatabase();
            this.migrationService = migrationService ?? new AccountMigrationService(this.database);
        }

        public List<AccountRecord> GetAllAccounts()
        {
            AccountListData data = database.LoadAccounts();
            data.accounts.Sort((left, right) => string.Compare(left.username, right.username, StringComparison.OrdinalIgnoreCase));
            return data.accounts;
        }

        public bool HasPendingLegacyImport()
        {
            return migrationService.HasPendingLegacyData();
        }

        public AccountRegisterResult Register(string username, string password, bool rememberLogin, out string message)
        {
            string normalizedUsername = NormalizeUsername(username);
            if (!IsValidUsername(normalizedUsername))
            {
                message = "用户名长度需为 3-20，且不能包含首尾空格。";
                return AccountRegisterResult.InvalidUsername;
            }

            if (!IsValidPassword(password))
            {
                message = "密码长度需为 6-32。";
                return AccountRegisterResult.InvalidPassword;
            }

            AccountListData data = database.LoadAccounts();
            if (FindByUsername(data.accounts, normalizedUsername) != null)
            {
                message = "用户名已存在。";
                return AccountRegisterResult.UsernameAlreadyExists;
            }

            bool shouldImportLegacyData = data.accounts.Count == 0 && migrationService.HasPendingLegacyData();
            string salt = PasswordHasher.CreateSaltBase64();
            AccountRecord record = new()
            {
                accountId = Guid.NewGuid().ToString("N"),
                username = normalizedUsername,
                passwordSalt = salt,
                passwordHash = PasswordHasher.HashPassword(password, salt),
                createdAtUtc = DateTime.UtcNow.ToString("o"),
                lastLoginAtUtc = DateTime.UtcNow.ToString("o")
            };

            data.accounts.Add(record);
            database.SaveAccounts(data);
            Directory.CreateDirectory(AccountPathProvider.GetAccountRoot(record.accountId));

            if (shouldImportLegacyData)
            {
                migrationService.TryImportPendingLegacyData(record.accountId);
            }

            AccountSession.Set(record);
            PersistCurrentAccount(record, rememberLogin);
            message = shouldImportLegacyData ? "注册成功，已导入旧存档。" : "注册成功。";
            return AccountRegisterResult.Success;
        }

        public AccountLoginResult Login(string username, string password, bool rememberLogin, out string message)
        {
            string normalizedUsername = NormalizeUsername(username);
            AccountListData data = database.LoadAccounts();
            AccountRecord record = FindByUsername(data.accounts, normalizedUsername);
            if (record == null || !PasswordHasher.VerifyPassword(password, record.passwordSalt, record.passwordHash))
            {
                message = "用户名或密码错误。";
                return AccountLoginResult.InvalidUsernameOrPassword;
            }

            record.lastLoginAtUtc = DateTime.UtcNow.ToString("o");
            database.SaveAccounts(data);
            AccountSession.Set(record);
            PersistCurrentAccount(record, rememberLogin);
            message = "登录成功。";
            return AccountLoginResult.Success;
        }

        public bool Logout()
        {
            AccountSession.Clear();
            database.ClearCurrentAccount();
            return true;
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword, out string message)
        {
            string normalizedUsername = NormalizeUsername(username);
            if (!IsValidPassword(newPassword))
            {
                message = "新密码长度需为 6-32。";
                return false;
            }

            AccountListData data = database.LoadAccounts();
            AccountRecord record = FindByUsername(data.accounts, normalizedUsername);
            if (record == null || !PasswordHasher.VerifyPassword(oldPassword, record.passwordSalt, record.passwordHash))
            {
                message = "用户名或密码错误。";
                return false;
            }

            string newSalt = PasswordHasher.CreateSaltBase64();
            record.passwordSalt = newSalt;
            record.passwordHash = PasswordHasher.HashPassword(newPassword, newSalt);
            database.SaveAccounts(data);
            message = "密码修改成功。";
            return true;
        }

        public bool DeleteAccount(string username, string password, out string message)
        {
            string normalizedUsername = NormalizeUsername(username);
            AccountListData data = database.LoadAccounts();
            AccountRecord record = FindByUsername(data.accounts, normalizedUsername);
            if (record == null || !PasswordHasher.VerifyPassword(password, record.passwordSalt, record.passwordHash))
            {
                message = "用户名或密码错误。";
                return false;
            }

            if (AccountSession.IsLoggedIn && AccountSession.CurrentAccountId == record.accountId)
            {
                message = "请先退出当前账号后再删除。";
                return false;
            }

            data.accounts.Remove(record);
            database.SaveAccounts(data);

            string accountRoot = AccountPathProvider.GetAccountRoot(record.accountId);
            if (Directory.Exists(accountRoot))
            {
                Directory.Delete(accountRoot, true);
            }

            CurrentAccountData currentAccount = database.LoadCurrentAccount();
            if (currentAccount != null && currentAccount.accountId == record.accountId)
            {
                database.ClearCurrentAccount();
            }

            message = "账号已删除。";
            return true;
        }

        public bool TryRestoreRememberedLogin()
        {
            CurrentAccountData currentAccount = database.LoadCurrentAccount();
            if (currentAccount == null || !currentAccount.rememberLogin || string.IsNullOrWhiteSpace(currentAccount.accountId))
            {
                AccountSession.Clear();
                return false;
            }

            AccountListData data = database.LoadAccounts();
            AccountRecord record = data.accounts.Find(item => item.accountId == currentAccount.accountId);
            if (record == null)
            {
                database.ClearCurrentAccount();
                AccountSession.Clear();
                return false;
            }

            AccountSession.Set(record);
            return true;
        }

        private void PersistCurrentAccount(AccountRecord record, bool rememberLogin)
        {
            if (!rememberLogin || record == null)
            {
                database.ClearCurrentAccount();
                return;
            }

            database.SaveCurrentAccount(new CurrentAccountData
            {
                accountId = record.accountId,
                rememberLogin = true
            });
        }

        private static AccountRecord FindByUsername(List<AccountRecord> accounts, string normalizedUsername)
        {
            return accounts.Find(account =>
                string.Equals(account.username, normalizedUsername, StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeUsername(string username)
        {
            return string.IsNullOrWhiteSpace(username) ? string.Empty : username.Trim();
        }

        private static bool IsValidUsername(string username)
        {
            return !string.IsNullOrEmpty(username) &&
                   username.Length >= MinUsernameLength &&
                   username.Length <= MaxUsernameLength;
        }

        private static bool IsValidPassword(string password)
        {
            return !string.IsNullOrEmpty(password) &&
                   password.Length >= MinPasswordLength &&
                   password.Length <= MaxPasswordLength;
        }
    }
}
