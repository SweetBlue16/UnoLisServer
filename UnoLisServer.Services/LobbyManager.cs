using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Contracts.Models;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services.Helpers;
using UnoLisServer.Services.ManagerInterfaces;
using UnoLisServer.Services.Validators;

namespace UnoLisServer.Services
{
    public class LobbyManager : ILobbyManager
    {
        private readonly LobbySessionHelper _sessionHelper;
        private readonly IPlayerRepository _playerRepository;
        private readonly ILobbyInvitationHelper _invitationHelper;
        private readonly IGameManager _gameManager;

        public LobbyManager(LobbySessionHelper sessionHelper, IPlayerRepository playerRepo, 
            ILobbyInvitationHelper invitationHelper, IGameManager gameManager)
        {
            _sessionHelper = sessionHelper;
            _playerRepository = playerRepo;
            _invitationHelper = invitationHelper;
            _gameManager = gameManager;
        }

        public LobbyManager() : this(LobbySessionHelper.Instance, new PlayerRepository(), 
            new LobbyInvitationHelper(), new GameManager())
        {
        }

        public async Task<CreateMatchResponse> CreateLobbyAsync(MatchSettings settings)
        {
            try
            {
                LobbyValidator.ValidateSettings(settings);

                string code = GenerateUniqueLobbyCode();
                var lobbyInfo = new LobbyInfo(code, settings);

                string hostAvatar = await GetAvatarAsync(settings.HostNickname);

                lobbyInfo.AddPlayer(settings.HostNickname, hostAvatar);
                _sessionHelper.AddLobby(code, lobbyInfo);

                Logger.Log($"[LOBBY] Created {code} by {settings.HostNickname}");

                return new CreateMatchResponse
                {
                    Success = true,
                    LobbyCode = code,
                    Message = "Lobby created successfully."
                };
            }
            catch (ArgumentException argEx)
            {
                Logger.Warn($"[LOBBY] Invalid settings creating lobby: {argEx.Message}");
                return new CreateMatchResponse { Success = false, Message = argEx.Message };
            }
            catch (SqlException sqlEx)
            {
                Logger.Error($"[LOBBY] SQL Error creating lobby for {settings?.HostNickname}", sqlEx);
                return new CreateMatchResponse { Success = false, Message = "Service temporarily unavailable (Database error)." };
            }
            catch (TimeoutException timeEx)
            {
                Logger.Error($"[LOBBY] Timeout creating lobby for {settings?.HostNickname}", timeEx);
                return new CreateMatchResponse { Success = false, Message = "Server busy. Please try again." };
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Critical error creating lobby", ex);
                return new CreateMatchResponse { Success = false, Message = "Internal server error." };
            }
        }

        public async Task<JoinMatchResponse> JoinLobbyAsync(string lobbyCode, string nickname)
        {
            try
            {
                LobbyValidator.ValidateJoinRequest(lobbyCode, nickname);

                var lobby = _sessionHelper.GetLobby(lobbyCode);
                if (lobby == null)
                {
                    Logger.Warn($"[LOBBY] Join attempt failed: Lobby {lobbyCode} not found.");
                    return new JoinMatchResponse { Success = false, Message = "Lobby not found." };
                }

                string avatar = await GetAvatarAsync(nickname);
                bool joined = lobby.AddPlayer(nickname, avatar);

                if (!joined)
                {
                    Logger.Warn($"[LOBBY] Join attempt failed: {nickname} -> {lobbyCode} (Lobby full or user exists).");
                    return new JoinMatchResponse { Success = false, Message = "Lobby full or player already exists." };
                }

                Logger.Log($"[LOBBY] {nickname} successfully joined {lobbyCode}");
                return new JoinMatchResponse
                {
                    Success = true,
                    LobbyCode = lobbyCode,
                    Message = "Joined successfully."
                };
            }
            catch (ArgumentException argEx)
            {
                Logger.Warn($"[LOBBY] Invalid join data: {argEx.Message}");
                return new JoinMatchResponse { Success = false, Message = argEx.Message };
            }
            catch (SqlException sqlEx)
            {
                Logger.Error($"[LOBBY] SQL Error during join for {nickname}", sqlEx);
                return new JoinMatchResponse { Success = false, Message = "Service unavailable (Database error)." };
            }
            catch (TimeoutException timeEx)
            {
                Logger.Error($"[LOBBY] Timeout during join for {nickname}", timeEx);
                return new JoinMatchResponse { Success = false, Message = "Server busy. Please try again." };
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Critical error joining lobby {lobbyCode}", ex);
                return new JoinMatchResponse { Success = false, Message = "Internal server error." };
            }
        }

        public Task<bool> SetLobbyBackgroundAsync(string lobbyCode, string backgroundName)
        {
            var lobby = _sessionHelper.GetLobby(lobbyCode);
            if (lobby != null && !string.IsNullOrWhiteSpace(backgroundName))
            {
                lobby.SelectedBackgroundVideo = backgroundName;
                Logger.Log($"[LOBBY] Background for {lobbyCode} set to {backgroundName}");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public LobbySettings GetLobbySettings(string lobbyCode)
        {
            var lobby = _sessionHelper.GetLobby(lobbyCode);
            if (lobby != null)
            {
                return new LobbySettings
                {
                    BackgroundVideoName = lobby.SelectedBackgroundVideo,
                    UseSpecialRules = lobby.Settings.UseSpecialRules
                };
            }
            return null;
        }

        public async Task<bool> SendInvitationsAsync(string lobbyCode, string senderNickname, List<string> invitedNicknames)
        {
            return await _invitationHelper.SendInvitationsAsync(lobbyCode, senderNickname, invitedNicknames);
        }

        public void RegisterConnection(string lobbyCode, string nickname)
        {
            if (OperationContext.Current == null)
            {
                Logger.Warn($"[LOBBY] RegisterConnection called without OperationContext for {nickname}");
                return;
            }

            try
            {
                var lobbyCallback = OperationContext.Current.GetCallbackChannel<ILobbyDuplexCallback>();
                _sessionHelper.RegisterCallback(lobbyCode, nickname, lobbyCallback);

                var lobby = _sessionHelper.GetLobby(lobbyCode);
                if (lobby == null)
                {
                    Logger.Warn($"[LOBBY] Player {nickname} registered connection but lobby {lobbyCode} not found.");
                    return;
                }

                SendInitialStateToPlayer(lobbyCallback, lobby, nickname);

                var lobbyPlayer = lobby.Players.FirstOrDefault(player => player.Nickname == nickname);
                string avatar = lobbyPlayer?.AvatarName ?? "LogoUNO";

                _sessionHelper.BroadcastToLobby(lobbyCode, call => call.PlayerJoined(nickname, avatar));
                _sessionHelper.BroadcastToLobby(lobbyCode, call => call.UpdatePlayerList(lobby.Players.ToArray()));
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[LOBBY] Communication error registering {nickname}: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[LOBBY] Timeout registering {nickname}: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Critical connection error for {nickname}", ex);
            }
        }

        private void SendInitialStateToPlayer(ILobbyDuplexCallback callback, LobbyInfo lobby, string nickname)
        {
            try
            {
                callback.UpdatePlayerList(lobby.Players.ToArray());
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[LOBBY] Failed to send initial state to {nickname}: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[LOBBY] Timeout sending initial state to {nickname}: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Unexpected error sending state to {nickname}", ex);
            }
        }

        public void RemoveConnection(string lobbyCode, string nickname, ILobbyDuplexCallback cachedCallback = null)
        {
            try
            {
                _sessionHelper.UnregisterCallback(lobbyCode, nickname);
                RemovePlayerAndNotify(lobbyCode, nickname);
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[LOBBY] WCF Communication error removing {nickname}: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[LOBBY] Timeout removing {nickname}: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Critical error removing connection for {nickname}", ex);
            }
        }

        private void RemovePlayerAndNotify(string lobbyCode, string nickname)
        {
            var lobby = _sessionHelper.GetLobby(lobbyCode);

            if (lobby == null)
            {
                return;
            }

            lobby.RemovePlayer(nickname);
            _sessionHelper.BroadcastToLobby(lobbyCode, call => call.PlayerLeft(nickname));
            _sessionHelper.BroadcastToLobby(lobbyCode, call => call.UpdatePlayerList(lobby.Players.ToArray()));

            Logger.Log($"[LOBBY] {nickname} removed from {lobbyCode} and notification sent.");
        }

        public async Task HandleReadyStatusAsync(string lobbyCode, string nickname, bool isReady)
        {
            var lobby = _sessionHelper.GetLobby(lobbyCode);
            if (lobby == null)
            {
                return;
            }

            bool allReady = false;

            lock (lobby.LobbyLock)
            {
                var player = lobby.Players.FirstOrDefault(p => p.Nickname == nickname);
                if (player != null) player.IsReady = isReady;

                if (lobby.Players.Count >= 2 && lobby.Players.All(lobbyPlayer => lobbyPlayer.IsReady))
                {
                    allReady = true;
                }
            }

            _sessionHelper.BroadcastToLobby(lobbyCode, call => call.PlayerReadyStatusChanged(nickname, isReady));

            if (allReady)
            {
                Logger.Log($"[LOBBY] All ready in {lobbyCode}. Countdown started.");
                await Task.Delay(3000);

                if (_sessionHelper.LobbyExists(lobbyCode) && lobby.Players.Count >= 2)
                {
                    var playerNicks = lobby.Players.Select(lobbyPlayer => lobbyPlayer.Nickname).ToList();
                    bool gameCreated = await _gameManager.InitializeGameAsync(lobbyCode, playerNicks);

                    if (gameCreated)
                    {
                        Logger.Log($"[LOBBY] Game session initialized for {lobbyCode}. Broadcasting start.");
                        _sessionHelper.BroadcastToLobby(lobbyCode, callback => callback.GameStarted());
                    }
                    else
                    {
                        Logger.Error($"[LOBBY] Failed to initialize game session for {lobbyCode}. Aborting start.");
                    }
                }
                else
                {
                    Logger.Warn($"[LOBBY] Start aborted in {lobbyCode}: Lobby removed or players left during countdown.");
                }
            }
        }

        private async Task<string> GetAvatarAsync(string nickname)
        {
            if (UserHelper.IsGuest(nickname))
            {
                return "LogoUNO";
            }

            try
            {
                var player = await _playerRepository.GetPlayerWithDetailsAsync(nickname);
                if (player?.SelectedAvatar_Avatar_idAvatar != null)
                {
                    var unlocked = player.AvatarsUnlocked
                        .FirstOrDefault(au => au.Avatar_idAvatar == player.SelectedAvatar_Avatar_idAvatar);
                    if (unlocked?.Avatar != null) return unlocked.Avatar.avatarName;
                }
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[LOBBY] Communication error resolving callback: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[LOBBY] Timeout resolving callback: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Unexpected error resolving callback from context", ex);
            }
            return "LogoUNO";
        }

        private string GenerateUniqueLobbyCode()
        {
            string code;
            do
            {
                code = SecureRandom.GetRandomString(5);
            }
            while (_sessionHelper.LobbyExists(code));
            return code;
        }
    }
}