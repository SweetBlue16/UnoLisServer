using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Contracts;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Services.ManagerInterfaces;

namespace UnoLisServer.Services
{
    /// <summary>
    /// Facade for logic manager for game
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = 
        ConcurrencyMode.Reentrant)]

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

        public void DisconnectPlayer(string lobbyCode, string nickname)
        {
            _gameManager.DisconnectPlayer(lobbyCode, nickname);
        }

        public void UseItem(string lobbyCode, string nickname, ItemType itemType, string targetNickname)
        {
            Task.Run(() =>
            {
                _gameManager.UseItem(lobbyCode, nickname, itemType, targetNickname);
            });
        }
    }
}
