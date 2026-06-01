using System;

namespace DesktopPet.Accounts
{
    [Serializable]
    public class MigrationStateData
    {
        public bool legacyDataStaged;
        public string importedAccountId;
    }
}
