using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services.Validators;

namespace UnoLisServer.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class FriendsManager : IFriendsManager, IDisposable
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IFriendsCallback _callback;
        private bool _disposed = false;

        public FriendsManager() : this(new FriendRepository())
        {
            _callback = OperationContext.Current.GetCallbackChannel<IFriendsCallback>();
            OperationContext.Current.Channel.Closed += (s, e) => Dispose();
        }

        public FriendsManager(IFriendRepository friendRepository, IFriendsCallback callbackTest = null)
        {
            _friendRepository = friendRepository;
            _callback = callbackTest;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        public void SubscribeToFriendUpdates(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            try
            {
                if (_callback == null)
                {
                    Logger.Warn($"[FRIENDS] Cannot subscribe. Callback channel is null.");
                    return;
                }

                SessionManager.AddSession(nickname, _callback);
                Logger.Log($"[FRIENDS] Player subscribed to updates.");
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[WCF] Communication error subscribing: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[WCF] Timeout subscribing: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error subscribing", ex);
            }
        }

        public void UnsubscribeFromFriendUpdates(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            try
            {
                SessionManager.RemoveSession(nickname);
                Logger.Log($"[FRIENDS] Player unsubscribed.");
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[FRIENDS] Communication error unsubscribing: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] Timeout unsubscribing: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[FRIENDS] Error unsubscribing", ex);
            }
        }

        public async Task<List<FriendData>> GetFriendsListAsync(string nickname)
        {
            if (UserHelper.IsGuest(nickname))
            {
                return new List<FriendData>();
            }

            try
            {
                var friendsEntities = await _friendRepository.GetFriendsEntitiesAsync(nickname);

                var result = friendsEntities.Select(friend => new FriendData
                {
                    FriendNickname = friend.nickname,
                    IsOnline = SessionManager.IsOnline(friend.nickname),
                    StatusMessage = "Friend"
                }).ToList();

                return result;
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Failed fetching friend list. Data Store unavailable.", ex);
                return new List<FriendData>();
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Timeout fetching friend list.");
                return new List<FriendData>();
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error fetching friend list.", ex);
                return new List<FriendData>();
            }
        }

        public async Task<List<FriendRequestData>> GetPendingRequestsAsync(string nickname)
        {
            if (UserHelper.IsGuest(nickname))
            {
                return new List<FriendRequestData>();
            }

            try
            {
                var requestEntities = await _friendRepository.GetPendingRequestsEntitiesAsync(nickname);

                return requestEntities.Select(request => new FriendRequestData
                {
                    RequesterNickname = request.Player?.nickname ?? "Unknown",
                    TargetNickname = nickname,
                    FriendListId = request.idFriendList
                }).ToList();
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] Timeout getting pending requests: {timeEx.Message}");
                return new List<FriendRequestData>();
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Failed fetching pending requests. Data Store unavailable.", ex);
                return new List<FriendRequestData>();
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Timeout fetching pending requests.");
                return new List<FriendRequestData>();
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error fetching pending requests.", ex);
                return new List<FriendRequestData>();
            }
        }

        public async Task<FriendRequestResult> SendFriendRequestAsync(string requesterNickname, string targetNickname)
        {
            if (string.IsNullOrWhiteSpace(requesterNickname) || string.IsNullOrWhiteSpace(targetNickname))
            {

            }

            if (IsGuestAction(requesterNickname, targetNickname))
            {
                return FriendRequestResult.Failed;
            }

            if (requesterNickname.Equals(targetNickname, StringComparison.OrdinalIgnoreCase))
            {
                return FriendRequestResult.CannotAddSelf;
            }

            try
            {
                FriendsValidator.ValidateNicknames(requesterNickname, targetNickname);

                var (requester, target) = await GetPlayersAsync(requesterNickname, targetNickname);

                if (IsInvalidPlayer(requester)|| IsInvalidPlayer(target))
                {
                    return FriendRequestResult.UserNotFound;
                }

                var relationshipStatus = await CheckExistingRelationshipAsync(requester.idPlayer, target.idPlayer);
                if (relationshipStatus != FriendRequestResult.Success)
                {
                    return relationshipStatus;
                }

                await CreateAndNotifyRequestAsync(requester, target);

                Logger.Log($"[FRIENDS] Request sent successfully from {requesterNickname} to {targetNickname}");
                return FriendRequestResult.Success;
            }
            catch (ArgumentException argEx)
            {
                Logger.Warn($"[FRIENDS] Invalid request arguments: {argEx.Message}");
                return FriendRequestResult.Failed;
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] Timeout sending request: {timeEx.Message}");
                return FriendRequestResult.Failed;
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Failed sending friend request. DB Unavailable.", ex);
                return FriendRequestResult.Failed;
            }
            catch (Exception ex) when (ex.Message == "Data_Conflict")
            {
                Logger.Warn($"[DATA] Conflict creating friend request. Request likely exists.");
                return FriendRequestResult.RequestAlreadySent;
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Timeout sending friend request.");
                return FriendRequestResult.Failed;
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error sending request", ex);
                return FriendRequestResult.Failed;
            }
        }

        private bool IsGuestAction(string requester, string target)
        {
            if (requester == null || target == null)
            {
                return false;
            }

            if (UserHelper.IsGuest(requester) || UserHelper.IsGuest(target))
            {
                Logger.Warn($"[FRIENDS] Guest attempt to friend request");
                return true;
            }
            return false;
        }

        private bool IsInvalidPlayer(Player player)
        {
            return player == null || player.idPlayer == 0;
        }

        private async Task<(Player requester, Player target)> GetPlayersAsync(string reqNick, string targetNick)
        {
            var requester = await _friendRepository.GetPlayerByNicknameAsync(reqNick);
            var target = await _friendRepository.GetPlayerByNicknameAsync(targetNick);

            return (requester, target);
        }

        private async Task<FriendRequestResult> CheckExistingRelationshipAsync(int requesterId, int targetId)
        {
            var existingRel = await _friendRepository.GetFriendshipEntryAsync(requesterId, targetId);
            return AnalyzeRelationshipStatus(existingRel, requesterId);
        }

        private async Task CreateAndNotifyRequestAsync(Player requester, Player target)
        {
            var newRequest = await _friendRepository.CreateFriendRequestAsync(requester.idPlayer, target.idPlayer);

            var requestData = new FriendRequestData
            {
                RequesterNickname = requester.nickname,
                TargetNickname = target.nickname,
                FriendListId = newRequest.idFriendList
            };

            try
            {
                NotifyRequestReceived(requestData);
            }
            catch (Exception ex)
            {
                Logger.Warn($"[FRIENDS-NOTIFY] Could not send real-time notification: {ex.Message}");
            }
        }

        private FriendRequestResult AnalyzeRelationshipStatus(FriendList existingRel, int requesterId)
        {
            if (existingRel == null)
            {
                return FriendRequestResult.Success;
            }

            if (existingRel.friendRequest == true)
            {
                return FriendRequestResult.AlreadyFriends;
            }

            if (existingRel.Player_idPlayer == requesterId)
            {
                return FriendRequestResult.RequestAlreadySent;
            }

            return FriendRequestResult.RequestAlreadyReceived;
        }

        public async Task<bool> AcceptFriendRequestAsync(FriendRequestData request)
        {
            if (request == null)
            {
                return false;
            }

            try
            {
                var (requester, target) = await GetPlayersAsync(request.RequesterNickname, request.TargetNickname);

                if (IsInvalidPlayer(requester) || IsInvalidPlayer(target))
                {
                    return false;
                }

                var relation = await _friendRepository.GetFriendshipEntryAsync(requester.idPlayer, target.idPlayer);

                if (!IsValidRequestToAccept(relation))
                {
                    Logger.Warn($"[FRIENDS] Attempt to accept invalid/existing request between two players.");
                    return false;
                }

                await ConfirmAndNotifyAcceptance(relation.idFriendList, request.RequesterNickname, 
                    request.TargetNickname);

                return true;
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] Timeout accepting request: {timeEx.Message}");
                return false;
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Accept request failed. Data Store unavailable.", ex);
                return false;
            }
            catch (Exception ex) when (ex.Message == "Data_Conflict")
            {
                Logger.Warn($"[DATA] Conflict accepting request. It might have been modified concurrently.");
                return false;
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Timeout accepting request.");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error accepting request.", ex);
                return false;
            }
        }

        public async Task<bool> RejectFriendRequestAsync(FriendRequestData request)
        {
            if (request == null)
            {
                return false;
            }

            try
            {
                var (requester, target) = await GetPlayersAsync(request.RequesterNickname, request.TargetNickname);

                if (IsInvalidPlayer(requester) || IsInvalidPlayer(target))
                {
                    return false;
                }

                var relation = await _friendRepository.GetFriendshipEntryAsync(requester.idPlayer, target.idPlayer);

                if (relation == null)
                {
                    Logger.Warn($"[FRIENDS] Cannot reject request: Relation not found.");
                    return false;
                }

                await _friendRepository.RemoveFriendshipEntryAsync(relation.idFriendList);
                return true;
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] Timeout rejecting request: {timeEx.Message}");
                return false;
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Reject request failed. Data Store unavailable.", ex);
                return false;
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Timeout rejecting request.");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error rejecting request.", ex);
                return false;
            }
        }

        private bool IsValidRequestToAccept(FriendList relation)
        {
            if (relation == null)
            {
                return false;
            }

            if (relation.friendRequest == true)
            {
                return false;
            }

            return true;
        }

        private async Task ConfirmAndNotifyAcceptance(int relationId, string requesterNick, string targetNick)
        {
            await _friendRepository.AcceptFriendRequestAsync(relationId);
            await NotifyFriendshipUpdateAsync(requesterNick, targetNick);
        }

        public async Task<bool> RemoveFriendAsync(FriendRequestData request)
        {
            if (request == null)
            {
                return false;
            }

            try
            {
                var (requester, target) = await GetPlayersAsync(request.RequesterNickname, request.TargetNickname);

                if (IsInvalidPlayer(requester) || IsInvalidPlayer(target))
                {
                    Logger.Warn($"[FRIENDS] Remove failed: One of the users not found.");
                    return false;
                }

                var relation = await _friendRepository.GetFriendshipEntryAsync(requester.idPlayer, target.idPlayer);

                if (!IsActiveFriendship(relation))
                {
                    Logger.Warn($"[FRIENDS] Attempt to remove non-friend relationship or relation not found.");
                    return false;
                }

                await ExecuteRemovalAndNotify(relation.idFriendList, request.RequesterNickname, 
                    request.TargetNickname);

                return true;
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] Timeout removing friend: {timeEx.Message}");
                return false;
            }
            catch (Exception ex) when (ex.Message == "DataStore_Unavailable")
            {
                Logger.Error($"[CRITICAL] Remove friend failed. Data Store unavailable.", ex);
                return false;
            }
            catch (Exception ex) when (ex.Message == "Server_Busy")
            {
                Logger.Warn($"[WARN] Timeout removing friend.");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error removing friend.", ex);
                return false;
            }
        }

        private bool IsActiveFriendship(FriendList relation)
        {
            if (relation == null)
            {
                return false;
            }

            return relation.friendRequest == true;
        }

        private async Task ExecuteRemovalAndNotify(int relationId, string requesterNick, string targetNick)
        {
            await _friendRepository.RemoveFriendshipEntryAsync(relationId);
            await NotifyFriendshipUpdateAsync(requesterNick, targetNick);
        }

        private static void NotifyRequestReceived(FriendRequestData request)
        {
            TryNotifyCallback(request.TargetNickname, callback =>
            {
                callback.FriendRequestReceived(request);
                Logger.Log($"[FRIENDS-NOTIFY] Notification sent");
            });
        }

        private async Task NotifyFriendshipUpdateAsync(string requesterNickname, string targetNickname)
        {
            await NotifyUserOfListUpdateAsync(requesterNickname);
            await NotifyUserOfListUpdateAsync(targetNickname);
        }

        private async Task NotifyUserOfListUpdateAsync(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            try
            {
                var updatedList = await GetFriendsListAsync(nickname);

                if (updatedList == null || updatedList.Count == 0)
                {
                    Logger.Warn($"[FRIENDS-NOTIFY] Aborted notification. Friends list retrieved is null.");
                    return;
                }

                TryNotifyCallback(nickname, callback =>
                {
                    callback.FriendListUpdated(updatedList);
                });

            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS-NOTIFY] Operation timed out: {timeEx.Message}");
            }
            catch (AggregateException aggEx)
            {
                foreach (var inner in aggEx.InnerExceptions)
                {
                    Logger.Error($"[FRIENDS-NOTIFY] Async error: {inner.Message}", inner);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[FRIENDS-NOTIFY] Unexpected error updating list", ex);
            }
        }

        private static void TryNotifyCallback(string nickname, Action<IFriendsCallback> action)
        {
            if (!SessionManager.IsOnline(nickname))
            {
                return;
            }
            
            var callback = SessionManager.GetSession(nickname) as IFriendsCallback;

            if (callback == null)
            {
                return;
            }

            try
            {
                var commObj = callback as ICommunicationObject;
                if (commObj != null && commObj.State == CommunicationState.Opened)
                {
                    action.Invoke(callback);
                }
                else
                {
                    Logger.Warn($"Skipping notification to {nickname}: Channel not Opened ({commObj?.State})");
                    SessionManager.RemoveSession(nickname);
                }
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"Communication failed notifying: {commEx.Message}");
                SessionManager.RemoveSession(nickname);
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"Timeout notifying: {timeEx.Message}");
                SessionManager.RemoveSession(nickname);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error notifying", ex);
            }
        }
    }
}