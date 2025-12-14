using System.Collections.Generic;
using System.ServiceModel;
using UnoLisServer.Common.Enums;
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

        [OperationContract(IsOneWay = true)]
        void SayUnoAsync(string lobbyCode, string nickname);

        [OperationContract(IsOneWay = true)]
        void DisconnectPlayer(string lobbyCode, string nickname);

        [OperationContract(IsOneWay = true)]
        void LeaveGame(string lobbyCode, string nickname);

        [OperationContract(IsOneWay = true)]
        void UseItem(string lobbyCode, string nickname, ItemType itemType, string targetNickname);
    }

    [ServiceContract]
    public interface IGameplayCallback : ISessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void CardPlayed(string nickname, Card card, int cardsLeft);

        [OperationContract(IsOneWay = true)]
        void CardDrawn(string nickname, int cardsLeft);

        [OperationContract(IsOneWay = true)]
        void TurnChanged(string nextPlayerNickname);

        [OperationContract(IsOneWay = true)]
        void MatchEnded(List<ResultData> results);

        [OperationContract(IsOneWay = true)]
        void ReceiveInitialHand(List<Card> hand);

        [OperationContract(IsOneWay = true)]
        void ReceiveCards(List<Card> cards);

        [OperationContract(IsOneWay = true)]
        void ReceivePlayerList(List<GamePlayer> players);

        [OperationContract(IsOneWay = true)]
        void GameMessage(string message);

        [OperationContract(IsOneWay = true)]
        void PlayerShoutedUno(string nickname);

    }
}

