using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    /// <summary>
    /// Interfaz exclusiva para el jugador que actúa como host de una partida.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IPartyHostCallback), SessionMode = SessionMode.Required)]
    public interface IPartyHostManager
    {
        [OperationContract(IsOneWay = true)]
        void CreateParty(string hostNickname);

        [OperationContract(IsOneWay = true)]
        void StartMatch(int partyId, string hostNickname);

        [OperationContract(IsOneWay = true)]
        void CancelParty(int partyId);
    }

    /// <summary>
    /// Callbacks exclusivos para el host.
    /// </summary>
    [ServiceContract]
    public interface IPartyHostCallback : ISessionCallback
    {
        [OperationContract]
        void PartyCreated(int partyId, string joinCode);

        [OperationContract]
        void PlayerJoined(string nickname);

        [OperationContract]
        void PlayerLeft(string nickname);

        [OperationContract]
        void PlayerReadyStatusChanged(string nickname, bool isReady);

        [OperationContract]
        void AllPlayersReady(); 

        [OperationContract]
        void PartyCancelled(); 

        [OperationContract]
        void MatchStarted();
    }
}
