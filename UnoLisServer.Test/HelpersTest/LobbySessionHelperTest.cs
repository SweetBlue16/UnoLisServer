using Moq;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Contracts.Models;
using UnoLisServer.Services.Helpers;
using Xunit;

namespace UnoLisServer.Test
{
    public class LobbySessionHelperTest
    {
        private readonly LobbySessionHelper _helper;

        public LobbySessionHelperTest()
        {
            _helper = LobbySessionHelper.Instance;
        }

        [Fact]
        public void AddLobby_NewCode_StoresLobby()
        {
            string code = Guid.NewGuid().ToString();
            var lobby = new LobbyInfo(code, new MatchSettings());

            _helper.AddLobby(code, lobby);

            Assert.True(_helper.LobbyExists(code));
            Assert.Same(lobby, _helper.GetLobby(code));
        }

        [Fact]
        public void AddLobby_DuplicateCode_DoesNotOverwriteOrThrow()
        {
            string code = Guid.NewGuid().ToString();
            var lobby1 = new LobbyInfo(code, new MatchSettings());
            var lobby2 = new LobbyInfo(code, new MatchSettings());

            _helper.AddLobby(code, lobby1);
            _helper.AddLobby(code, lobby2); 

            Assert.Same(lobby1, _helper.GetLobby(code));
        }

        [Fact]
        public void RemoveLobby_ExistingCode_RemovesIt()
        {
            string code = Guid.NewGuid().ToString();
            _helper.AddLobby(code, new LobbyInfo(code, new MatchSettings()));

            _helper.RemoveLobby(code);

            Assert.False(_helper.LobbyExists(code));
            Assert.Null(_helper.GetLobby(code));
        }

        [Fact]
        public void BroadcastToLobby_NoClients_DoesNotThrow()
        {
            string code = Guid.NewGuid().ToString();
            _helper.AddLobby(code, new LobbyInfo(code, new MatchSettings()));

            var exception = Record.Exception(() =>
                _helper.BroadcastToLobby(code, cb => cb.GameStarted()));

            Assert.Null(exception);
        }

        [Fact]
        public void BroadcastToLobby_LobbyDoesNotExist_DoesNotThrow()
        {
            var exception = Record.Exception(() =>
                _helper.BroadcastToLobby("GHOST_CODE", cb => cb.GameStarted()));

            Assert.Null(exception);
        }
    }
}