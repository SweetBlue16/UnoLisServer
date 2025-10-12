using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Common.Constants
{
    public static class Endpoints
    {
        public const string AuthEndpoint = "net.tcp://localhost:8000/AuthService";
        public const string ProfileEndpoint = "net.tcp://localhost:8001/ProfileService";
        public const string FriendsEndpoint = "net.tcp://localhost:8002/FriendsService";
        public const string ShopEndpoint = "net.tcp://localhost:8003/ShopService";
        public const string MatchmakingEndpoint = "net.tcp://localhost:8004/MatchmakingService";
        public const string GameplayEndpoint = "net.tcp://localhost:8005/GameplayService";
        public const string NotificationsEndpoint = "net.tcp://localhost:8006/NotificationsService";
    }
}
