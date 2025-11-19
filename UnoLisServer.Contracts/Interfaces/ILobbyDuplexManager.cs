using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(ILobbyDuplexCallback))]
    public interface ILobbyDuplexManager
    {
        [OperationContract]
        void ConnectToLobby(string lobbyCode, string nickname);

        [OperationContract]
        void DisconnectFromLobby(string lobbyCode, string nickname);

        [OperationContract]
        void SetReadyStatus(string lobbyCode, string nickname, bool isReady);
    }
    [ServiceContract]
    public interface ILobbyDuplexCallback
    {
        [OperationContract(IsOneWay = true)]
        void PlayerJoined(string nickname, string avatarName);

        [OperationContract(IsOneWay = true)]
        void PlayerLeft(string nickname);

        [OperationContract(IsOneWay = true)]
        void UpdatePlayerList(LobbyPlayerData[] nicknames);

        [OperationContract(IsOneWay = true)]
        void PlayerReadyStatusChanged(string nickname, bool isReady);

        [OperationContract(IsOneWay = true)]
        void GameStarted();
    }
}
