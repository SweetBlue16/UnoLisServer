using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Services.ManagerInterfaces;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class MatchmakingManager : IMatchmakingManager
    {
        private readonly ILobbyManager _lobbyManager;

        public MatchmakingManager() : this(new LobbyManager())
        {
        }

        public MatchmakingManager(ILobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
        }

        public async Task<CreateMatchResponse> CreateMatchAsync(MatchSettings settings)
        {
            return await _lobbyManager.CreateLobbyAsync(settings);
        }

        public async Task<JoinMatchResponse> JoinMatchAsync(string lobbyCode, string nickname)
        {
            return await _lobbyManager.JoinLobbyAsync(lobbyCode, nickname);
        }

        public async Task<bool> SetLobbyBackgroundAsync(string lobbyCode, string backgroundName)
        {
            return await _lobbyManager.SetLobbyBackgroundAsync(lobbyCode, backgroundName);
        }

        public async Task<LobbySettings> GetLobbySettingsAsync(string lobbyCode)
        {
            var settings = _lobbyManager.GetLobbySettings(lobbyCode);
            return await Task.FromResult(settings ?? new LobbySettings());
        }

        public async Task<bool> SendInvitationsAsync(string lobbyCode, string senderNickname, List<string> invitedNicknames)
        {
            return await _lobbyManager.SendInvitationsAsync(lobbyCode, senderNickname, invitedNicknames);
        }
    }
}