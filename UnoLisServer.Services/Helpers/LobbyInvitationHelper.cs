using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;   

namespace UnoLisServer.Services.Helpers
{
    public interface ILobbyInvitationHelper
    {
        Task<bool> SendInvitationsAsync(string lobbyCode, string senderNickname, List<string> invitedNicknames);
    }

    public class LobbyInvitationHelper : ILobbyInvitationHelper
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly INotificationSender _notificationSender;

        public LobbyInvitationHelper(IPlayerRepository playerRepo, INotificationSender notificationSender)
        {
            _playerRepository = playerRepo;
            _notificationSender = notificationSender;
        }

        public LobbyInvitationHelper() : this(new PlayerRepository(), NotificationSender.Instance)
        {
        }

        public async Task<bool> SendInvitationsAsync(string lobbyCode, string senderNickname, List<string> invitedNicknames)
        {
            if (invitedNicknames == null || !invitedNicknames.Any())
            {
                return false;
            }

            try
            {
                var emailsToSend = await ResolveEmailsAsync(invitedNicknames);

                if (!emailsToSend.Any())
                {
                    Logger.Warn($"[INVITATIONS] No valid emails found for lobby {lobbyCode}");
                    return false;
                }

                await DispatchEmailsAsync(emailsToSend, senderNickname, lobbyCode);

                Logger.Log($"[INVITATIONS] Sent for {lobbyCode} to {emailsToSend.Count} users.");
                return true;
            }
            catch (AggregateException aggEx)
            {
                foreach (var inner in aggEx.InnerExceptions)
                {
                    Logger.Error($"[INVITATIONS] Async error: {inner.Message}", inner);
                }
                return false;
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[INVITATIONS] Timeout for {lobbyCode}: {timeEx.Message}");
                return false;
            }
            catch (SqlException sqlEx)
            {
                Logger.Error($"[INVITATIONS] DB error", sqlEx);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"[INVITATIONS] Critical error for {lobbyCode}", ex);
                return false;
            }
        }

        private async Task<List<string>> ResolveEmailsAsync(List<string> nicknames)
        {
            var emails = new List<string>();
            var tasks = new List<Task<Player>>();

            foreach (var nick in nicknames)
            {
                tasks.Add(_playerRepository.GetPlayerWithDetailsAsync(nick));
            }

            var players = await Task.WhenAll(tasks);

            foreach (var player in players)
            {
                var email = player?.Account?.FirstOrDefault()?.email;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    emails.Add(email);
                }
            }
            return emails;
        }

        private async Task DispatchEmailsAsync(List<string> emails, string sender, string lobbyCode)
        {
            var tasks = emails.Select(email =>
                _notificationSender.SendMatchInvitationAsync(email, sender, lobbyCode));

            await Task.WhenAll(tasks);
        }
    }
}