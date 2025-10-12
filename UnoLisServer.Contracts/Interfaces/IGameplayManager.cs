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
        void DrawCard(string nickname);
    }

    [ServiceContract]
    public interface IGameplayCallback : ISessionCallback
    {
        [OperationContract]
        void CardPlayed(string nickname, Card card);

        [OperationContract]
        void CardDrawn(string nickname);

        [OperationContract]
        void TurnChanged(string nextPlayerNickname);

        [OperationContract]
        void MatchEnded(List<ResultData> results);

    }
}

