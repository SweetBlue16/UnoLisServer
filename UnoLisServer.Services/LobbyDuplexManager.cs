using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Common.Helpers;

namespace UnoLisServer.Services
{
    /// <summary>
    /// Per session service to manage real-time duplex communication for lobby events.
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class LobbyDuplexManager : ILobbyDuplexManager
    {
        public void ConnectToLobby(string lobbyCode, string nickname)
        {
            Logger.Log($"Request to connect duplex: {nickname} -> {lobbyCode}");
            LobbyManager.Instance.RegisterPlayerConnection(lobbyCode, nickname);
        }

        public void DisconnectFromLobby(string lobbyCode, string nickname)
        {
            LobbyManager.Instance.RemovePlayerConnection(lobbyCode, nickname);
        }

        public void SetReadyStatus(string lobbyCode, string nickname, bool isReady)
        {
            LobbyManager.Instance.BroadcastReadyStatus(lobbyCode, nickname, isReady);
        }
    }
}
