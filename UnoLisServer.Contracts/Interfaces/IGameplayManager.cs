using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IGameplayCallback), SessionMode = SessionMode.Required)]
    public interface IGameplayManager
    {
        [OperationContract(IsOneWay = true)]
        void PlayCard(PlayCardData data);

        [OperationContract(IsOneWay = true)]
        void DrawCard(string lobbyCode, string nickname);

        [OperationContract(IsOneWay = true)]
        void ConnectToGame(string lobbyCode, string nickname);
    }

    [ServiceContract]
    public interface IGameplayCallback : ISessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void CardPlayed(string nickname, Card card);

        [OperationContract(IsOneWay = true)]
        void CardDrawn(string nickname);

        [OperationContract(IsOneWay = true)]
        void TurnChanged(string nextPlayerNickname);

        [OperationContract(IsOneWay = true)]
        void MatchEnded(List<ResultData> results);

        [OperationContract(IsOneWay = true)]
        void ReceiveInitialHand(List<Card> hand);

        [OperationContract(IsOneWay = true)]
        void ReceiveCards(List<Card> cards);

        [OperationContract(IsOneWay = true)]
        void ReceivePlayerList(List<string> players);

    }
}

