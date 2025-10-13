using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Common.Constants
{
    public static class Endpoints
    {
        // AUTENTICACIÓN
        public const string AuthEndpoint = "net.tcp://localhost:8101/Auth";

        // PERFIL
        public const string ProfileEndpoint = "net.tcp://localhost:8103/Profile";

        // AMIGOS
        public const string FriendsEndpoint = "net.tcp://localhost:8105/Friends";

        // TIENDA
        public const string ShopEndpoint = "net.tcp://localhost:8107/Shop";

        // LOBBY / PARTIDAS
        public const string PartyHostEndpoint = "net.tcp://localhost:8107/PartyHost";
        public const string PartyClientEndpoint = "net.tcp://localhost:8109/PartyClient";

        // GAMEPLAY
        public const string GameplayEndpoint = "net.tcp://localhost:8111/Gameplay";

        // NOTIFICACIONES
        public const string NotificationsEndpoint = "net.tcp://localhost:8113/Notifications";
    }
}