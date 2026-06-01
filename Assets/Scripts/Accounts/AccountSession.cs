namespace DesktopPet.Accounts
{
    public static class AccountSession
    {
        public static string CurrentAccountId { get; private set; }
        public static string CurrentUsername { get; private set; }

        public static bool IsLoggedIn => !string.IsNullOrEmpty(CurrentAccountId);

        public static void Set(AccountRecord record)
        {
            if (record == null)
            {
                Clear();
                return;
            }

            CurrentAccountId = record.accountId;
            CurrentUsername = record.username;
        }

        public static void Clear()
        {
            CurrentAccountId = null;
            CurrentUsername = null;
        }
    }
}
