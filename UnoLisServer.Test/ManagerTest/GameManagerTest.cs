using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Enums;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services;
using UnoLisServer.Services.GameLogic;
using UnoLisServer.Services.GameLogic.Models;
using UnoLisServer.Services.Helpers;
using Xunit;

namespace UnoLisServer.Test.ManagerTest
{
    public class GameManagerTest
    {
        private readonly Mock<IGameSessionHelper> _mockSessionHelper;
        private readonly Mock<IPlayerRepository> _mockRepository;
        private readonly Mock<IGameplayCallback> _mockCallback;

        public GameManagerTest()
        {
            _mockSessionHelper = new Mock<IGameSessionHelper>();
            _mockRepository = new Mock<IPlayerRepository>();
            _mockCallback = new Mock<IGameplayCallback>();
        }

        private GameManager CreateManager()
        {
            return new GameManager(_mockSessionHelper.Object, _mockRepository.Object);
        }

        private GameSession CreateTestSession(string lobbyCode, string playerTurn, List<GamePlayerData> players)
        {
            var session = new GameSession(lobbyCode, players);
            while (session.GetCurrentPlayer().Nickname != playerTurn)
            {
                session.NextTurn();
            }
            return session;
        }

        [Fact]
        public async Task TestPlayCardSessionEmptyShouldReturnWithoutAction()
        {
            _mockSessionHelper.Setup(s => s.GetGame(It.IsAny<string>())).Returns(GameSession.Empty);
            var manager = CreateManager();

            var context = new PlayCardContext("Lobby1", "User1", "1", null);

            await manager.PlayCardAsync(context);

            _mockSessionHelper.Verify(s => s.BroadcastToGame(It.IsAny<string>(),
                It.IsAny<Action<IGameplayCallback>>()), Times.Never);
        }

        [Fact]
        public async Task TestPlayCardNotPlayerTurnShouldNotExecuteMove()
        {
            var player1 = new GamePlayerData
            {
                Nickname = "P1",
                Hand = new List<Card> { new Card { Id = "1", Color =
                CardColor.Red, Value = CardValue.Five } }
            };
            var player2 = new GamePlayerData { Nickname = "P2" };
            var session = CreateTestSession("Lobby1", "P1", new List<GamePlayerData> { player1, player2 });

            _mockSessionHelper.Setup(s => s.GetGame("Lobby1")).Returns(session);
            var manager = CreateManager();

            var context = new PlayCardContext("Lobby1", "P2", "1", null);

            await manager.PlayCardAsync(context);

            _mockSessionHelper.Verify(s => s.BroadcastToGame(It.IsAny<string>(),
                It.IsAny<Action<IGameplayCallback>>()), Times.Never);
        }

        [Fact]
        public async Task TestPlayCardValidMoveShouldRemoveCardFromHand()
        {
            var cardToPlay = new Card { Id = "10", Color = CardColor.Blue, Value = CardValue.One };
            var player1 = new GamePlayerData { Nickname = "P1", Hand = new List<Card> { cardToPlay } };
            var session = CreateTestSession("Lobby1", "P1", new List<GamePlayerData> { player1 });

            session.CurrentActiveColor = CardColor.Blue;

            _mockSessionHelper.Setup(s => s.GetGame("Lobby1")).Returns(session);
            var manager = CreateManager();

            var context = new PlayCardContext("Lobby1", "P1", "10", null);

            await manager.PlayCardAsync(context);

            Assert.DoesNotContain(player1.Hand, c => c.Id == "10");
        }

        [Fact]
        public async Task TestPlayCardValidMoveShouldBroadcastPlay()
        {
            var cardToPlay = new Card { Id = "10", Color = CardColor.Blue, Value = CardValue.One };
            var player1 = new GamePlayerData
            {
                Nickname = "P1",
                Hand = new List<Card> { cardToPlay,
                new Card { Id = "20" } }
            };
            var session = CreateTestSession("Lobby1", "P1", new List<GamePlayerData> { player1 });
            session.CurrentActiveColor = CardColor.Blue;

            _mockSessionHelper.Setup(s => s.GetGame("Lobby1")).Returns(session);

            _mockSessionHelper.Setup(s => s.BroadcastToGame("Lobby1", It.IsAny<Action<IGameplayCallback>>()))
                .Callback<string, Action<IGameplayCallback>>((code, action) => action(_mockCallback.Object));

            var manager = CreateManager();
            var context = new PlayCardContext("Lobby1", "P1", "10", null);

            await manager.PlayCardAsync(context);

            _mockCallback.Verify(cb => cb.CardPlayed("P1", It.Is<Card>(c => c.Id == "10"), 1), Times.Once);
        }

        [Fact]
        public async Task TestPlayCardWinConditionShouldTriggerMatchEnd()
        {
            var cardToPlay = new Card { Id = "99", Color = CardColor.Green, Value = CardValue.Nine };
            var player1 = new GamePlayerData { Nickname = "Winner", Hand = new List<Card> { cardToPlay } };
            var session = CreateTestSession("LobbyWin", "Winner", new List<GamePlayerData> { player1 });
            session.CurrentActiveColor = CardColor.Green;

            _mockSessionHelper.Setup(s => s.GetGame("LobbyWin")).Returns(session);

            _mockSessionHelper.Setup(s => s.BroadcastToGame("LobbyWin", It.IsAny<Action<IGameplayCallback>>()))
                .Callback<string, Action<IGameplayCallback>>((code, action) => action(_mockCallback.Object));

            var manager = CreateManager();
            var context = new PlayCardContext("LobbyWin", "Winner", "99", null);

            await manager.PlayCardAsync(context);

            _mockCallback.Verify(cb => cb.MatchEnded(It.IsAny<List<ResultData>>()), Times.Once);
        }
    }
}