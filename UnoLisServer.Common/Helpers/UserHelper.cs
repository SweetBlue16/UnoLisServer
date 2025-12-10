using System;

namespace UnoLisServer.Common.Helpers
{
    public static class UserHelper
    {
        public static bool IsGuest(string nickname)
        {
            return nickname.StartsWith("Guest_", StringComparison.OrdinalIgnoreCase);
        }
    }
}
