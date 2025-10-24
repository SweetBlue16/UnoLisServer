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
    /// Interfaz que implementan los jugadores invitados a una partida existente.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IPartyClientCallback), SessionMode = SessionMode.Required)]
    public interface IPartyClientManager
    {
        [OperationContract(IsOneWay = true)]
        void JoinParty(JoinPartyRequest request); //NOTA Modificar los métodos de forma en que se les pase un DTO y evitar tantos callbacks

        [OperationContract(IsOneWay = true)]
        void LeaveParty(PartyActionData data);

        [OperationContract(IsOneWay = true)]
        void SetReadyStatus(PartyActionData data);
    }

    /// <summary>
    /// Callbacks que notifican al jugador sobre el estado de la partida.
    /// </summary>
    [ServiceContract]
    public interface IPartyClientCallback : ISessionCallback
    {
        [OperationContract]
        void JoinedSuccessfully(int partyId, string hostNickname);

        [OperationContract]
        void JoinFailed(string reason);

        [OperationContract]
        void PlayerJoined(string nickname);

        [OperationContract]
        void PlayerLeft(string nickname);

        [OperationContract]
        void PlayerReadyStatusChanged(string nickname, bool isReady);

        [OperationContract]
        void MatchStarting(); 

        [OperationContract]
        void PartyCancelled(); 

        [OperationContract]
        void PartyDisbanded();
    }
}
