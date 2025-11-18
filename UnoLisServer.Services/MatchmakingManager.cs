using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Services.ManagerInterfaces;

namespace UnoLisServer.Services
{
    /// <summary>
    /// WCF Service implementation that acts as the entry point.
    /// It is testable by delegating all logic to an injected ILobbyManager.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class MatchmakingManager : IMatchmakingManager
    {
        private readonly ILobbyManager _lobbyManager;

        public MatchmakingManager() : this(LobbyManager.Instance) 
        {
        }

        public MatchmakingManager(ILobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
        }

        public Task<CreateMatchResponse> CreateMatchAsync(MatchSettings settings)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (settings == null || string.IsNullOrEmpty(settings.HostNickname))
                    {
                        return new CreateMatchResponse { Success = false,
                            Message = "Invalid host.",
                            LobbyCode = null
                        };
                    }
                    if (settings.MaxPlayers < 2 || settings.MaxPlayers > 4)
                    {
                        return new CreateMatchResponse
                        {
                            Success = false,
                            Message = "Invalid player count.",
                            LobbyCode = null
                        };
                    }

                    var response = _lobbyManager.CreateLobby(settings);
                    return response;
                }
                catch (TimeoutException ex)
                {
                    Logger.Error($"Timeout in CreateMatchAsync: {ex.Message}", ex);
                    return new CreateMatchResponse { 
                        Success = false, 
                        Message = "The server took too long to respond.", 
                        LobbyCode = null};
                }
                catch (CommunicationException ex)
                {
                    Logger.Error($"Communication Error in CreateMatchAsync: {ex.Message}", ex);
                    return new CreateMatchResponse { 
                        Success = false, 
                        Message = "Network error contacting server.",
                        LobbyCode = null};
                }
                catch (Exception ex)
                {
                    Logger.Error($"Fatal error in CreateMatchAsync: {ex.Message}", ex);
                    return new CreateMatchResponse
                    {
                        Success = false,
                        Message = "Internal server error processing the request.",
                        LobbyCode = null
                    };
                }
            });
        }

        public Task<JoinMatchResponse> JoinMatchAsync(string lobbyCode, string nickname)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(nickname))
                    {
                        return new JoinMatchResponse
                        {
                            Success = false,
                            Message = "Invalid data.",
                            LobbyCode = null
                        };
                    }
                
                    return _lobbyManager.JoinLobby(lobbyCode, nickname);
                }
                catch (TimeoutException ex)
                {
                    Logger.Error($"Timeout in JoinMatchAsync: {ex.Message}", ex);
                    return new JoinMatchResponse
                    {
                        Success = false,
                        Message = "The server took too long to respond.",
                        LobbyCode = null
                    };
                }
                catch (CommunicationException ex)
                {
                    Logger.Error($"Communication Error in JoinMatchAsync: {ex.Message}", ex);
                    return new JoinMatchResponse
                    {
                        Success = false,
                        Message = "Network error contacting server.",
                        LobbyCode = null
                    };
                }
                catch (Exception ex)
                {
                    Logger.Error($"Fatal error in JoinMatchAsync: {ex.Message}", ex);
                    return new JoinMatchResponse
                    {
                        Success = false,
                        Message = "Internal server error processing the request.",
                        LobbyCode = null
                    };
                }
            });
        }
    }
}
