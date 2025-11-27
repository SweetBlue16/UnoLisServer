using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
