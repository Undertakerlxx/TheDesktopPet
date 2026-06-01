using System;
using System.IO;
using UnityEngine;

namespace DesktopPet.Accounts
{
    public class AccountDatabase
    {
        public AccountListData LoadAccounts()
        {
            if (!File.Exists(AccountPathProvider.AccountsFilePath))
            {
                return new AccountListData();
            }

            try
            {
                string json = File.ReadAllText(AccountPathProvider.AccountsFilePath);
                AccountListData data = JsonUtility.FromJson<AccountListData>(json) ?? new AccountListData();
                data.EnsureCollections();
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"AccountDatabase: failed to load accounts. {exception.Message}");
                return new AccountListData();
            }
        }

        public void SaveAccounts(AccountListData data)
        {
            data ??= new AccountListData();
            data.EnsureCollections();

            try
            {
                Directory.CreateDirectory(AccountPathProvider.AccountsRootPath);
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(AccountPathProvider.AccountsFilePath, json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"AccountDatabase: failed to save accounts. {exception.Message}");
            }
        }

        public CurrentAccountData LoadCurrentAccount()
        {
            if (!File.Exists(AccountPathProvider.CurrentAccountFilePath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(AccountPathProvider.CurrentAccountFilePath);
                return JsonUtility.FromJson<CurrentAccountData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"AccountDatabase: failed to load current account. {exception.Message}");
                return null;
            }
        }

        public void SaveCurrentAccount(CurrentAccountData data)
        {
            if (data == null)
            {
                ClearCurrentAccount();
                return;
            }

            try
            {
                Directory.CreateDirectory(AccountPathProvider.AccountsRootPath);
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(AccountPathProvider.CurrentAccountFilePath, json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"AccountDatabase: failed to save current account. {exception.Message}");
            }
        }

        public void ClearCurrentAccount()
        {
            try
            {
                if (File.Exists(AccountPathProvider.CurrentAccountFilePath))
                {
                    File.Delete(AccountPathProvider.CurrentAccountFilePath);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"AccountDatabase: failed to clear current account. {exception.Message}");
            }
        }
    }
}
