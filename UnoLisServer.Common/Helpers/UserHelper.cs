using System;

namespace UnoLisServer.Common.Helpers
{
    /// <summary>
    /// Class that provides helper methods for user-related operations.
    /// </summary>
    public static class UserHelper
    {
        public static bool IsGuest(string nickname)
        {
            return nickname.StartsWith("Guest_", StringComparison.OrdinalIgnoreCase);
        }
    }
}
