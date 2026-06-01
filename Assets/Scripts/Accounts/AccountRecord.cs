using System;

namespace DesktopPet.Accounts
{
    [Serializable]
    public class AccountRecord
    {
        public string accountId;
        public string username;
        public string passwordHash;
        public string passwordSalt;
        public string createdAtUtc;
        public string lastLoginAtUtc;
    }
}
