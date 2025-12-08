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
    /// <summary>
    /// Facade for managing matchmaking operations such as creating and joining matches.
    /// </summary>
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
            if (settings == null)
            {
                Logger.Warn("[MATCHMAKING] CreateMatch called with null settings.");
                return new CreateMatchResponse { Success = false, Message = "Invalid settings." };
            }

            try
            {
                return await _lobbyManager.CreateLobbyAsync(settings);
            }
            catch (ArgumentException argEx)
            {
                Logger.Warn($"[MATCHMAKING] Bad Request creating match: {argEx.Message}");
                return new CreateMatchResponse { Success = false, Message = argEx.Message };
            }
            catch (TimeoutException timeEx)
            {
                Logger.Error($"[MATCHMAKING] Timeout delegating CreateMatch for host {settings.HostNickname}", timeEx);
                return new CreateMatchResponse { Success = false, Message = "Server is busy." };
            }
            catch (Exception ex)
            {
                Logger.Error($"[MATCHMAKING] Critical error in CreateMatchAsync", ex);
                return new CreateMatchResponse { Success = false, Message = "Internal Server Error." };
            }
        }

        public async Task<JoinMatchResponse> JoinMatchAsync(string lobbyCode, string nickname)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
            {
                return new JoinMatchResponse { Success = false, Message = "Invalid parameters." };
            }

            try
            {
                return await _lobbyManager.JoinLobbyAsync(lobbyCode, nickname);
            }
            catch (ArgumentException argEx)
            {
                Logger.Warn($"[MATCHMAKING] Invalid join params: {argEx.Message}");
                return new JoinMatchResponse { Success = false, Message = argEx.Message };
            }
            catch (TimeoutException timeEx)
            {
                Logger.Error($"[MATCHMAKING] Timeout joining lobby {lobbyCode}", timeEx);
                return new JoinMatchResponse { Success = false, Message = "Server is busy." };
            }
            catch (Exception ex)
            {
                Logger.Error($"[MATCHMAKING] Critical error in JoinMatchAsync for {lobbyCode}", ex);
                return new JoinMatchResponse { Success = false, Message = "Internal Server Error." };
            }
        }

        public async Task<bool> SetLobbyBackgroundAsync(string lobbyCode, string backgroundName)
        {
            try
            {
                return await _lobbyManager.SetLobbyBackgroundAsync(lobbyCode, backgroundName);
            }
            catch (ArgumentException argEx)
            {
                Logger.Warn($"[MATCHMAKING] Invalid background: {argEx.Message}");
                return false;
            }
            catch (TimeoutException timeEx)
            {
                Logger.Error($"[MATCHMAKING] Timeout setting background in lobby {lobbyCode}", timeEx);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"[MATCHMAKING] Error setting background for {lobbyCode}", ex);
                return false;
            }
        }

        public async Task<LobbySettings> GetLobbySettingsAsync(string lobbyCode)
        {
            try
            {
                var settings = _lobbyManager.GetLobbySettings(lobbyCode);
                return await Task.FromResult(settings ?? new LobbySettings());
            }
            catch (TimeoutException timeEx)
            {
                Logger.Error($"[MATCHMAKING] Timeout setting background in lobby {lobbyCode}", timeEx);
                return new LobbySettings();
            }
            catch (Exception ex)
            {
                Logger.Error($"[MATCHMAKING] Error fetching settings for {lobbyCode}", ex);
                return new LobbySettings();
            }
        }

        public async Task<bool> SendInvitationsAsync(string lobbyCode, string senderNickname, List<string> invitedNicknames)
        {
            if (invitedNicknames == null || invitedNicknames.Count == 0)
            {
                return false;
            }

            try
            {
                return await _lobbyManager.SendInvitationsAsync(lobbyCode, senderNickname, invitedNicknames);
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[MATCHMAKING] Timeout sending invitations: {timeEx.Message}");
                return false;
            }
            catch (AggregateException aggEx)
            {
                foreach (var inner in aggEx.InnerExceptions)
                {
                    Logger.Error($"[MATCHMAKING] Async error sending invitations: {inner.Message}", inner);
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"[MATCHMAKING] Critical error sending invitations", ex);
                return false;
            }
        }
    }
}