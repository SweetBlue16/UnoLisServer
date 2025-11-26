using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Data.Repositories;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Services.Providers;
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
            if (_disposed) return;
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
                SessionManager.AddSession(nickname, _callback);
                Logger.Log($"[FRIENDS] Player {nickname} subscribed to updates.");
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[FRIENDS] Communication error subscribing {nickname}: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] Timeout subscribing {nickname}: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[FRIENDS] Unexpected error subscribing {nickname}", ex);
            }
        }

        public void UnsubscribeFromFriendUpdates(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname)) return;

            try
            {
                SessionManager.RemoveSession(nickname);
                Logger.Log($"[FRIENDS] Player {nickname} unsubscribed.");
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[FRIENDS] Communication error unsubscribing {nickname}: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] Timeout unsubscribing {nickname}: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[FRIENDS] Error unsubscribing {nickname}", ex);
            }
        }

        public async Task<List<FriendData>> GetFriendsListAsync(string nickname)
        {
            try
            {
                var friendsEntities = await _friendRepository.GetFriendsEntitiesAsync(nickname);

                var result = friendsEntities.Select(f => new FriendData
                {
                    FriendNickname = f.nickname,
                    IsOnline = SessionManager.IsOnline(f.nickname),
                    StatusMessage = "Friend"
                }).ToList();

                return result;
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] DB Timeout fetching list for {nickname}: {timeEx.Message}");
                return new List<FriendData>();
            }
            catch (SqlException sqlEx)
            {
                Logger.Error($"[FRIENDS] SQL Error fetching list for {nickname}", sqlEx);
                return new List<FriendData>();
            }
            catch (Exception ex)
            {
                Logger.Error($"[FRIENDS] Unexpected Error fetching list for {nickname}", ex);
                return new List<FriendData>();
            }
        }

        public async Task<List<FriendRequestData>> GetPendingRequestsAsync(string nickname)
        {
            try
            {
                var requestEntities = await _friendRepository.GetPendingRequestsEntitiesAsync(nickname);

                return requestEntities.Select(r => new FriendRequestData
                {
                    RequesterNickname = r.Player.nickname,
                    TargetNickname = nickname,
                    FriendListId = r.idFriendList
                }).ToList();
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] Timeout getting pending requests for {nickname}: {timeEx.Message}");
                return new List<FriendRequestData>();
            }
            catch (SqlException sqlEx)
            {
                Logger.Error($"[FRIENDS] SQL Error getting pending requests for {nickname}", sqlEx);
                return new List<FriendRequestData>();
            }
            catch (Exception ex)
            {
                Logger.Error($"[FRIENDS] Unexpected error getting pending requests for {nickname}", ex);
                return new List<FriendRequestData>();
            }
        }

        public async Task<FriendRequestResult> SendFriendRequestAsync(string requesterNickname, string targetNickname)
        {
            try
            {
                FriendsValidator.ValidateNicknames(requesterNickname, targetNickname);

                if (requesterNickname.Equals(targetNickname, StringComparison.OrdinalIgnoreCase))
                {
                    return FriendRequestResult.CannotAddSelf;
                }

                var requester = await _friendRepository.GetPlayerByNicknameAsync(requesterNickname);
                var target = await _friendRepository.GetPlayerByNicknameAsync(targetNickname);

                if (requester == null || target == null)
                {
                    return FriendRequestResult.UserNotFound;
                }

                var existingRel = await _friendRepository.GetFriendshipEntryAsync(requester.idPlayer, target.idPlayer);
                var statusCheck = AnalyzeRelationshipStatus(existingRel, requester.idPlayer);

                if (statusCheck != FriendRequestResult.Success)
                {
                    return statusCheck;
                }

                var newRequest = await _friendRepository.CreateFriendRequestAsync(requester.idPlayer, target.idPlayer);
                var requestData = new FriendRequestData
                {
                    RequesterNickname = requesterNickname,
                    TargetNickname = targetNickname,
                    FriendListId = newRequest.idFriendList
                };

                NotifyRequestReceived(requestData);
                Logger.Log($"[FRIENDS] Request sent: {requesterNickname} -> {targetNickname}");

                return FriendRequestResult.Success;
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] Timeout sending request {requesterNickname}->{targetNickname}: {timeEx.Message}");
                return FriendRequestResult.Failed;
            }
            catch (SqlException sqlEx)
            {
                Logger.Error($"[FRIENDS] SQL Error sending request {requesterNickname}->{targetNickname}", sqlEx);
                return FriendRequestResult.Failed;
            }
            catch (Exception ex)
            {
                Logger.Error($"[FRIENDS] Unexpected error sending request {requesterNickname}->{targetNickname}", ex);
                return FriendRequestResult.Failed;
            }
        }

        /// <summary>
        /// Auxiliar method to analyze existing relationship status.
        /// </summary>
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
            try
            {
                var requester = await _friendRepository.GetPlayerByNicknameAsync(request.RequesterNickname);
                var target = await _friendRepository.GetPlayerByNicknameAsync(request.TargetNickname);

                if (requester == null || target == null)
                {
                    return false;
                }

                var friendRelation = await _friendRepository.GetFriendshipEntryAsync(requester.idPlayer, target.idPlayer);

                if (friendRelation == null || friendRelation.friendRequest == true)
                {
                    Logger.Warn($"[FRIENDS] Attempt to accept invalid/existing request: {request.RequesterNickname}->{request.TargetNickname}");
                    return false;
                }

                await _friendRepository.AcceptFriendRequestAsync(friendRelation.idFriendList);
                await NotifyFriendshipUpdateAsync(request.RequesterNickname, request.TargetNickname);

                return true;
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] Timeout accepting request: {timeEx.Message}");
                return false;
            }
            catch (SqlException sqlEx)
            {
                Logger.Error($"[FRIENDS] SQL Error accepting request", sqlEx);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"[FRIENDS] Unexpected error accepting request", ex);
                return false;
            }
        }

        public async Task<bool> RejectFriendRequestAsync(FriendRequestData request)
        {
            try
            {
                var requester = await _friendRepository.GetPlayerByNicknameAsync(request.RequesterNickname);
                var target = await _friendRepository.GetPlayerByNicknameAsync(request.TargetNickname);

                if (requester == null || target == null)
                {
                    return false;
                }

                var friendRelation = await _friendRepository.GetFriendshipEntryAsync(requester.idPlayer, target.idPlayer);

                if (friendRelation == null)
                {
                    Logger.Warn($"[FRIENDS] Cannot reject request {request.RequesterNickname}->{request.TargetNickname}: Relation not found.");
                    return false;
                }

                await _friendRepository.RemoveFriendshipEntryAsync(friendRelation.idFriendList);

                Logger.Log($"[FRIENDS] Request rejected by {request.TargetNickname}");
                return true;
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] Timeout rejecting request: {timeEx.Message}");
                return false;
            }
            catch (SqlException sqlEx)
            {
                Logger.Error($"[FRIENDS] SQL Error rejecting request", sqlEx);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"[FRIENDS] Unexpected error rejecting request", ex);
                return false;
            }
        }

        public async Task<bool> RemoveFriendAsync(FriendRequestData request)
        {
            try
            {
                var requesterPlayer = await _friendRepository.GetPlayerByNicknameAsync(request.RequesterNickname);
                var targetPlayer = await _friendRepository.GetPlayerByNicknameAsync(request.TargetNickname);

                if (requesterPlayer == null || targetPlayer == null)
                {
                    return false;
                }

                var relation = await _friendRepository.GetFriendshipEntryAsync(requesterPlayer.idPlayer, targetPlayer.idPlayer);

                if (relation == null || relation.friendRequest != true)
                {
                    Logger.Warn($"[FRIENDS] Attempt to remove non-friend relationship: {request.RequesterNickname}<->{request.TargetNickname}");
                    return false;
                }

                await _friendRepository.RemoveFriendshipEntryAsync(relation.idFriendList);
                await NotifyFriendshipUpdateAsync(request.RequesterNickname, request.TargetNickname);
                Logger.Log($"[FRIENDS] Friendship removed: {request.RequesterNickname} and {request.TargetNickname}");
                
                return true;
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS] Timeout removing friend: {timeEx.Message}");
                return false;
            }
            catch (SqlException sqlEx)
            {
                Logger.Error($"[FRIENDS] SQL Error removing friend", sqlEx);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"[FRIENDS] Unexpected error removing friend", ex);
                return false;
            }
        }

        private static void NotifyRequestReceived(FriendRequestData request)
        {
            TryNotifyCallback(request.TargetNickname, callback =>
            {
                callback.FriendRequestReceived(request);
                Logger.Log($"[FRIENDS-NOTIFY] Notification sent to {request.TargetNickname}");
            });
        }

        private async Task NotifyFriendshipUpdateAsync(string requesterNickname, string targetNickname)
        {
            await NotifyUserOfListUpdateAsync(requesterNickname);
            await NotifyUserOfListUpdateAsync(targetNickname);
        }

        private async Task NotifyUserOfListUpdateAsync(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname)) return;

            try
            {
                var updatedList = await GetFriendsListAsync(nickname);

                if (updatedList == null)
                {
                    Logger.Warn($"[FRIENDS-NOTIFY] Aborted notification to {nickname}: Friends list retrieved is null.");
                    return;
                }

                TryNotifyCallback(nickname, callback =>
                {
                    callback.FriendListUpdated(updatedList);
                });

                Logger.Log($"[FRIENDS-NOTIFY] List update successfully sent to {nickname}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[FRIENDS-NOTIFY] Operation timed out for {nickname}: {timeEx.Message}");
            }
            catch (AggregateException aggEx)
            {
                foreach (var inner in aggEx.InnerExceptions)
                {
                    Logger.Error($"[FRIENDS-NOTIFY] Async error for {nickname}: {inner.Message}", inner);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[FRIENDS-NOTIFY] Critical error updating list for {nickname}", ex);
            }
        }

        private static void TryNotifyCallback(string nickname, Action<IFriendsCallback> action)
        {
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
                Logger.Warn($"Communication failed notifying {nickname}: {commEx.Message}");
                SessionManager.RemoveSession(nickname);
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"Timeout notifying {nickname}: {timeEx.Message}");
                SessionManager.RemoveSession(nickname);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error notifying {nickname}", ex);
            }
        }
    }
}