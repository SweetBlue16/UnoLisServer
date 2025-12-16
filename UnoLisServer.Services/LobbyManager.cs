using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Contracts.Models;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services.Helpers;
using UnoLisServer.Services.ManagerInterfaces;
using UnoLisServer.Services.Validators;

namespace UnoLisServer.Services
{
    /// <summary>
    /// Logic for Lobby acting as an orchestor
    /// </summary>
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
            if (settings == null)
            {
                return new CreateMatchResponse { Success = false, Message = "Invalid settings." };
            }

            try
            {
                LobbyValidator.ValidateSettings(settings);

                string code = GenerateUniqueLobbyCode();
                var lobbyInfo = new LobbyInfo(code, settings);

                string hostAvatar = await GetAvatarAsync(settings.HostNickname);

                lobbyInfo.AddPlayer(settings.HostNickname, hostAvatar);
                _sessionHelper.AddLobby(code, lobbyInfo);

                Logger.Log($"[LOBBY] Created {code}");

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
            catch (TimeoutException timeEx)
            {
                Logger.Error($"[LOBBY] Timeout creating lobby", timeEx);
                return new CreateMatchResponse { Success = false, Message = "Server busy. Please try again." };
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Create Lobby failed. Data Store unavailable.", ex);
                return new CreateMatchResponse { Success = false, Message = "Service temporarily unavailable." };
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Timeout creating lobby.");
                return new CreateMatchResponse { Success = false, Message = "Server busy. Please try again." };
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error creating lobby", ex);
                return new CreateMatchResponse { Success = false, Message = "Internal server error." };
            }
        }

        public async Task<JoinMatchResponse> JoinLobbyAsync(string lobbyCode, string nickname)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                return new JoinMatchResponse { Success = false, Message = "Invalid input." };
            }

            try
            {
                LobbyValidator.ValidateJoinRequest(lobbyCode, nickname);

                var lobby = _sessionHelper.GetLobby(lobbyCode);

                if (lobby == null)
                {
                    Logger.Warn($"[LOBBY] Join attempt failed: Lobby {lobbyCode} not found (Returned NULL).");
                    return new JoinMatchResponse { Success = false, Message = "Lobby not found." };
                }

                if (string.IsNullOrWhiteSpace(lobby.LobbyCode))
                {
                    Logger.Warn($"[LOBBY] Join attempt failed: Lobby {lobbyCode} not found.");
                    return new JoinMatchResponse { Success = false, Message = "Lobby not found." };
                }

                string avatar = await GetAvatarAsync(nickname);
                bool joined = lobby.AddPlayer(nickname, avatar);

                if (!joined)
                {
                    Logger.Warn($"[LOBBY] Join attempt failed: {lobbyCode} (Lobby full or user exists).");
                    return new JoinMatchResponse { Success = false, Message = "Lobby full or player already exists." };
                }

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
            catch (TimeoutException timeEx)
            {
                Logger.Error($"[LOBBY] Timeout during join", timeEx);
                return new JoinMatchResponse { Success = false, Message = "Server busy. Please try again." };
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Join Lobby failed. DB Unavailable.", ex);
                return new JoinMatchResponse { Success = false, Message = "Service unavailable." };
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Timeout joining lobby.");
                return new JoinMatchResponse { Success = false, Message = "Server busy." };
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Critical error joining lobby {lobbyCode}", ex);
                return new JoinMatchResponse { Success = false, Message = "Internal server error." };
            }
        }

        public Task<bool> SetLobbyBackgroundAsync(string lobbyCode, string backgroundName)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(backgroundName))
            {
                return Task.FromResult(false);
            }

            try
            {
                var lobby = _sessionHelper.GetLobby(lobbyCode);
                if (lobby != null && !string.IsNullOrWhiteSpace(lobby.LobbyCode))
                {
                    lobby.SelectedBackgroundVideo = backgroundName;
                    return Task.FromResult(true);
                }
            }
            catch (TimeoutException timeEx)
            {
                Logger.Error($"[LOBBY] Timeout setting background", timeEx);
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Unexpected error setting background for {lobbyCode}", ex);
            }

            return Task.FromResult(false);
        }

        public LobbySettings GetLobbySettings(string lobbyCode)
        {
            try
            {
                var lobby = _sessionHelper.GetLobby(lobbyCode);
                if (lobby != null && !string.IsNullOrWhiteSpace(lobby.LobbyCode))
                {
                    return new LobbySettings
                    {
                        BackgroundVideoName = lobby.SelectedBackgroundVideo,
                        UseSpecialRules = lobby.Settings.UseSpecialRules
                    };
                }
            }
            catch (TimeoutException timeEx)
            {
                Logger.Error($"[LOBBY] Timeout getting lobby Settings", timeEx);
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Unexpected error getting lobby settings for {lobbyCode}", ex);
            }

            return new LobbySettings();
        }

        public async Task<bool> SendInvitationsAsync(string lobbyCode, string senderNickname,
            List<string> invitedNicknames)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || invitedNicknames == null || !invitedNicknames.Any())
            {
                return false;
            }

            try
            {
                return await _invitationHelper.SendInvitationsAsync(lobbyCode, senderNickname, invitedNicknames);
            }
            catch (TimeoutException timeEx)
            {
                Logger.Error($"[LOBBY] Timeout sending invitations", timeEx);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Error sending invitations", ex);
                return false;
            }
        }

        public void RegisterConnection(string lobbyCode, string nickname)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            if (OperationContext.Current == null)
            {
                Logger.Warn($"[LOBBY] RegisterConnection called without OperationContext");
                return;
            }

            try
            {
                var lobbyCallback = OperationContext.Current.GetCallbackChannel<ILobbyDuplexCallback>();
                if (lobbyCallback == null)
                {
                    Logger.Warn($"[LOBBY] Callback channel is null. Cannot register.");
                    return;
                }

                _sessionHelper.RegisterCallback(lobbyCode, nickname, lobbyCallback);

                var lobby = _sessionHelper.GetLobby(lobbyCode);
                if (string.IsNullOrWhiteSpace(lobby.LobbyCode))
                {
                    Logger.Warn($"[LOBBY] Player registered connection but lobby {lobbyCode} not found.");
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
                Logger.Warn($"[LOBBY] Communication error registering: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[LOBBY] Timeout registering: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Critical connection error", ex);
            }
        }

        private void SendInitialStateToPlayer(ILobbyDuplexCallback callback, LobbyInfo lobby, string nickname)
        {
            if (callback == null || lobby == null)
            {
                return;
            }

            try
            {
                var playerList = lobby.Players.ToArray();
                callback.UpdatePlayerList(playerList);
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[LOBBY] Failed to send initial state: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[LOBBY] Timeout sending initial state: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Unexpected error sending state", ex);
            }
        }

        public void RemoveConnection(string lobbyCode, string nickname, ILobbyDuplexCallback cachedCallback = null)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            try
            {
                _sessionHelper.UnregisterCallback(lobbyCode, nickname);
                RemovePlayerAndNotify(lobbyCode, nickname);
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[LOBBY] WCF Communication error removing player: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[LOBBY] Timeout removing player: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Critical error removing connection", ex);
            }
        }

        private void RemovePlayerAndNotify(string lobbyCode, string nickname)
        {
            try
            {
                var lobby = _sessionHelper.GetLobby(lobbyCode);

                if (string.IsNullOrWhiteSpace(lobby.LobbyCode))
                {
                    Logger.Warn($"[LOBBY] Attempted to remove but lobby {lobbyCode} does not exist.");
                    return;
                }

                lobby.RemovePlayer(nickname);
                _sessionHelper.BroadcastToLobby(lobbyCode, call => call.PlayerLeft(nickname));

                var currentPlayers = lobby.Players.ToArray();
                _sessionHelper.BroadcastToLobby(lobbyCode, call => call.UpdatePlayerList(currentPlayers));
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[LOBBY] Communication error notifying removal: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[LOBBY] Timeout notifying removal: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Unexpected error removing player from lobby", ex);
            }
        }

        public async Task HandleReadyStatusAsync(string lobbyCode, string nickname, bool isReady)
        {
            int minPlayersToStart = 2;
            if (string.IsNullOrWhiteSpace(lobbyCode))
            {
                return;
            }

            try
            {
                var lobby = _sessionHelper.GetLobby(lobbyCode);
                if (string.IsNullOrWhiteSpace(lobby.LobbyCode))
                {
                    Logger.Warn($"[LOBBY] Ready status ignored. Lobby {lobbyCode} not found.");
                    return;
                }

                bool allReady = false;

                lock (lobby.LobbyLock)
                {
                    var player = lobby.Players.FirstOrDefault(lobbyPlayer => lobbyPlayer.Nickname == nickname);
                    if (player != null)
                    {
                        player.IsReady = isReady;
                    }

                    if (lobby.Players.Count >= minPlayersToStart && lobby.Players.All(lobbyPlayer => lobbyPlayer.IsReady))
                    {
                        allReady = true;
                    }
                }

                _sessionHelper.BroadcastToLobby(lobbyCode, call => call.PlayerReadyStatusChanged(nickname, isReady));

                if (allReady)
                {
                    await HandleGameStartSequenceAsync(lobby, lobbyCode);
                }
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[LOBBY] Communication error handling ready status: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[LOBBY] Timeout handling ready status: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Critical error handling ready status for {lobbyCode}", ex);
            }
        }

        private async Task HandleGameStartSequenceAsync(LobbyInfo lobby, string lobbyCode)
        {
            await Task.Delay(3000);

            bool isValidToStart = false;
            List<string> playerNicks = null;
            
            lock (lobby.LobbyLock)
            {
                if (!string.IsNullOrWhiteSpace(_sessionHelper.GetLobby(lobbyCode).LobbyCode) && lobby.Players.Count >= 2)
                {
                    if (lobby.Players.All(player => player.IsReady))
                    {
                        isValidToStart = true;
                        playerNicks = lobby.Players.Select(player => player.Nickname).ToList();
                    }
                    else
                    {
                        Logger.Log($"[LOBBY] Start aborted in {lobbyCode}. A player unreadied during countdown.");
                    }
                }
                else
                {
                    Logger.Warn($"[LOBBY] Start aborted in {lobbyCode}. Lobby closed or players left during countdown.");
                }
            }

            if (isValidToStart && playerNicks != null)
            {
                try
                {
                    bool gameCreated = await _gameManager.InitializeGameAsync(lobbyCode, playerNicks);

                    if (gameCreated)
                    {
                        Logger.Log($"[LOBBY] Game session initialized for {lobbyCode}. Broadcasting GameStarted.");
                        _sessionHelper.BroadcastToLobby(lobbyCode, callback => callback.GameStarted());
                    }
                    else
                    {
                        Logger.Error($"[LOBBY] Failed to initialize game logic for {lobbyCode}. Aborting.");
                    }
                }
                catch (CommunicationException commEx)
                {
                    Logger.Warn($"[LOBBY] Communication error initializing game: {commEx.Message}");
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Warn($"[LOBBY] Timeout initializing game: {timeEx.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Exception initializing game for {lobbyCode}", ex);
                }
            }
        }

        private async Task<string> GetAvatarAsync(string nickname)
        {
            const string DefaultAvatar = "LogoUNO";

            if (UserHelper.IsGuest(nickname))
            {
                return DefaultAvatar;
            }

            try
            {
                var player = await _playerRepository.GetPlayerWithDetailsAsync(nickname);
                if (player?.SelectedAvatar_Avatar_idAvatar != null && player.AvatarsUnlocked != null)
                {
                    var unlocked = player.AvatarsUnlocked
                        .FirstOrDefault(avatarsUnlocked => avatarsUnlocked.Avatar_idAvatar == player.SelectedAvatar_Avatar_idAvatar);

                    if (unlocked?.Avatar != null)
                    {
                        return unlocked.Avatar.avatarName;
                    }
                }
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Warn($"[LOBBY] Could not fetch avatar. DB Unavailable. Using default.");
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[LOBBY] Timeout fetching avatar. Using default.");
            }
            catch (Exception ex)
            {
                Logger.Error($"[LOBBY] Unexpected error resolving avatar", ex);
            }

            return DefaultAvatar;
        }

        private string GenerateUniqueLobbyCode()
        {
            string code;
            int attempts = 0;
            const int MaxAttempts = 100;

            do
            {
                code = SecureRandom.GetRandomString(5);
                attempts++;

                if (attempts > MaxAttempts)
                {
                    Logger.Error("[CRITICAL] Failed to generate unique lobby code after 100 attempts.");
                    throw new Exception("Server_Busy");
                }
            }
            while (_sessionHelper.LobbyExists(code));

            return code;
        }
    }
}