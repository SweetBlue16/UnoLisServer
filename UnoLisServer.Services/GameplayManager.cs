using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Services.ManagerInterfaces;
using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class GameplayManager : IGameplayManager
    {
        private readonly IGameManager _gameManager;

        public GameplayManager() : this(new GameManager())
        {
        }

        public GameplayManager(IGameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void PlayCard(PlayCardData data)
        {
            var context = new PlayCardContext(
                data.LobbyCode,
                data.Nickname,
                data.CardId,
                data.SelectedColorId
            );

            Task.Run(async () =>
            {
                await _gameManager.PlayCardAsync(context);
            });
        }

        public void DrawCard(string lobbyCode, string nickname)
        {
            Task.Run(async () =>
            {
                await _gameManager.DrawCardAsync(lobbyCode, nickname);
            });
        }

        public void ConnectToGame(string lobbyCode, string nickname)
        {
            _gameManager.ConnectPlayer(lobbyCode, nickname);
        }

        public void SayUnoAsync(string lobbyCode, string nickname)
        {
            Task.Run(async () =>
            {
                await _gameManager.SayUnoAsync(lobbyCode, nickname);
            });
        }
    }
}
