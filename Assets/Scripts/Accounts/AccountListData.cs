using System;
using System.Collections.Generic;

namespace DesktopPet.Accounts
{
    [Serializable]
    public class AccountListData
    {
        public List<AccountRecord> accounts = new();

        public void EnsureCollections()
        {
            accounts ??= new List<AccountRecord>();
        }
    }
}
