using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class GameplayManager : IGameplayManager
    {
        private readonly IGameplayCallback _callback;

        public GameplayManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IGameplayCallback>();
        }

        public void PlayCard(PlayCardData data)
        {
            _callback.CardPlayed(data.Nickname, data.PlayedCard);
        }

        public void DrawCard(string nickname)
        {
            _callback.CardDrawn(nickname);
        }

    }
}
