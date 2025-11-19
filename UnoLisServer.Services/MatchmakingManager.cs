using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
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

        public Task<bool> SendInvitationsAsync(string lobbyCode, string senderNickname, List<string> invitedNicknames)
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (invitedNicknames == null || !invitedNicknames.Any()) return false;

                    List<string> emailsToSend = GetEmailsForNicknames(invitedNicknames);

                    if (!emailsToSend.Any())
                    {
                        Logger.Log($"No valid emails found for invitation to lobby {lobbyCode}.");
                        return false;
                    }

                    await ExecuteEmailSendingAsync(lobbyCode, senderNickname, emailsToSend);

                    return true;
                }
                catch (TimeoutException ex)
                {
                    Logger.Error($"Timeout sending invitations for lobby {lobbyCode}: {ex.Message}", ex);
                    return false;
                }
                catch (SqlException ex)
                {
                    Logger.Error($"Database error sending invitations for lobby {lobbyCode}: {ex.Message}", ex);
                    return false;
                }
                catch (EntityException ex)
                {
                    Logger.Error($"Entity Framework error sending invitations for lobby {lobbyCode}: {ex.Message}", ex);
                    return false;
                }
                catch (CommunicationException ex)
                {
                    Logger.Error($"Communication error sending invitations for lobby {lobbyCode}: {ex.Message}", ex);
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unexpected error sending invitations for lobby {lobbyCode}: {ex.Message}", ex);
                    return false;
                }
            });
        }

        private List<string> GetEmailsForNicknames(List<string> invitedNicknames)
        {
            var emails = new List<string>();
            using (var context = new UNOContext())
            {
                var players = context.Player
                    .Include("Account")
                    .Where(p => invitedNicknames.Contains(p.nickname))
                    .ToList();

                foreach (var player in players)
                {
                    var account = player.Account.FirstOrDefault();
                    if (account != null && !string.IsNullOrEmpty(account.email))
                    {
                        emails.Add(account.email);
                    }
                }
            }
            return emails;
        }

        private async Task ExecuteEmailSendingAsync(string lobbyCode, string senderNickname, List<string> emailsToSend)
        {
            var tasks = emailsToSend.Select(email =>
                NotificationSender.Instance.SendMatchInvitationAsync(email, senderNickname, lobbyCode));

            await Task.WhenAll(tasks);

            Logger.Log($"Invitations sent for lobby {lobbyCode} from {senderNickname} to {emailsToSend.Count} recipients.");
        }
    }
}
