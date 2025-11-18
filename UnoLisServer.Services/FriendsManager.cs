using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Common.Helpers;

namespace UnoLisServer.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class FriendsManager : IFriendsManager, IDisposable
    {
        private readonly IFriendsCallback _callback;
        private readonly IFriendsLogicManager _logicManager;
        private bool _disposed = false;

        public FriendsManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IFriendsCallback>();
            _logicManager = new FriendsLogicManager();
            OperationContext.Current.Channel.Closed += (s, e) => Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
        public void SubscribeToFriendUpdates(string nickname)
        {
            try
            {
                SessionManager.AddSession(nickname, _callback);
                Logger.Log($"Player {nickname} subscribed to Friends updates.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error subscribing {nickname}: {ex.Message}", ex);
            }
        }

        public void UnsubscribeFromFriendUpdates(string nickname)
        {
            SessionManager.RemoveSession(nickname);
            Logger.Log($"Player {nickname} unsubscribed from Friends updates.");
        }

        public async Task<List<FriendData>> GetFriendsListAsync(string nickname)
        {
            try
            {
                return await _logicManager.GetFriendsListAsync(nickname);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in GetFriendsListAsync for {nickname}: {ex.Message}", ex);
                return new List<FriendData>();
            }
        }

        public async Task<List<FriendRequestData>> GetPendingRequestsAsync(string nickname)
        {
            try
            {
                return await _logicManager.GetPendingRequestsAsync(nickname);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in GetPendingRequestsAsync for {nickname}: {ex.Message}", ex);
                return new List<FriendRequestData>();
            }
        }

        public async Task<FriendRequestResult> SendFriendRequestAsync(string requesterNickname, string targetNickname)
        {
            try
            {
                var validationResult = await _logicManager.ValidateAndCheckExistingRequestAsync(
                    requesterNickname, targetNickname);

                if (validationResult.Result != FriendRequestResult.Success)
                {
                    return validationResult.Result;
                }

                var createRequestDto = new CreateRequestDto
                {
                    RequesterId = validationResult.RequesterId,
                    TargetId = validationResult.TargetId,
                    RequesterNickname = requesterNickname,
                    TargetNickname = targetNickname
                };

                FriendRequestData newRequestDTO = await _logicManager.CreatePendingRequestAsync(createRequestDto);

                NotifyRequestReceived(newRequestDTO);

                Logger.Log($"[FriendsManager] Request sent: {requesterNickname} -> {targetNickname}");
                return FriendRequestResult.Success;
            }
            catch (CommunicationException ex)
            {
                Logger.Error($"WCF Communication Error: {ex.Message}", ex);
                return FriendRequestResult.Failed;
            }
            catch (SqlException ex)
            {
                Logger.Error($"SQL Server Error: {ex.Message}", ex);
                return FriendRequestResult.Failed;
            }
            catch (EntityException ex)
            {
                Logger.Error($"Entity Framework Error (Commit/Validation): {ex.Message}", ex);
                return FriendRequestResult.Failed;
            }
            catch (Exception ex)
            {
                Logger.Error($"Fatal/Unexpected Error in SendFriendRequestAsync: {ex.Message}", ex);
                return FriendRequestResult.Failed;
            }
        }

        public async Task<bool> AcceptFriendRequestAsync(FriendRequestData request)
        {
            try
            {
                using (var context = new UNOContext())
                using (var transaction = context.Database.BeginTransaction())
                {
                    bool success = await _logicManager.AcceptRequestAsync(request);
                    if (success)
                    {
                        transaction.Commit();
                        await NotifyFriendshipAcceptedAsync(request.RequesterNickname, request.TargetNickname);
                        return true;
                    }
                    transaction.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in AcceptFriendRequestAsync: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> RejectFriendRequestAsync(FriendRequestData request)
        {
            try
            {
                using (var context = new UNOContext())
                using (var transaction = context.Database.BeginTransaction())
                {
                    bool success = await _logicManager.RejectPendingRequestAsync(
                        request.TargetNickname,
                        request.RequesterNickname);

                    if (success)
                    {
                        transaction.Commit();
                        Logger.Log($"Request rejected: {request.TargetNickname} rejected {request.RequesterNickname}");
                        return true;
                    }
                    transaction.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in RejectFriendRequestAsync: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> RemoveFriendAsync(FriendRequestData request)
        {
            try
            {
                using (var context = new UNOContext())
                using (var transaction = context.Database.BeginTransaction())
                {
                    bool success = await _logicManager.RemoveConfirmedFriendshipAsync(
                        request.RequesterNickname,
                        request.TargetNickname);

                    if (success)
                    {
                        transaction.Commit();
                        await NotifyFriendshipRemovedAsync(request.RequesterNickname, request.TargetNickname);
                        return true;
                    }
                    transaction.Rollback();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in RemoveFriendAsync: {ex.Message}", ex);
                return false;
            }
        }

        private async Task NotifyFriendshipAcceptedAsync(string requesterNickname, string targetNickname)
        {
            TryNotifyCallback(requesterNickname, cb =>
            {

                Task.Run(async () =>
                {
                    try
                    {
                        var friends = await _logicManager.GetFriendsListAsync(requesterNickname);
                        cb.FriendListUpdated(friends);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error in async callback for {requesterNickname}: {ex.Message}", ex);
                    }
                });
            });

            var targetFriends = await _logicManager.GetFriendsListAsync(targetNickname);
            _callback.FriendListUpdated(targetFriends);
        }

        private async Task NotifyFriendshipRemovedAsync(string removerNickname, string removedNickname)
        {
            TryNotifyCallback(removedNickname, cb =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var friends = await _logicManager.GetFriendsListAsync(removedNickname);
                        cb.FriendListUpdated(friends);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error in removed player callback for {removedNickname}: {ex.Message}", ex);
                    }
                });
            });

            try
            {
                var friends = await _logicManager.GetFriendsListAsync(removerNickname);
                _callback.FriendListUpdated(friends);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating remover's list ({removerNickname}): {ex.Message}", ex);
            }
        }

        private static void TryNotifyCallback(string nickname, Action<IFriendsCallback> action)
        {
            var cb = SessionManager.GetSession(nickname) as IFriendsCallback;

            if (cb == null)
            {
                Logger.Log($"Skipping notification to {nickname}: Player is offline or not subscribed.");
                return;
            }

            try
            {
                action.Invoke(cb);

                var communicationObject = cb as ICommunicationObject;
                if (communicationObject != null && communicationObject.State != CommunicationState.Opened)
                {
                    throw new CommunicationException($"Channel to {nickname} is in state: {communicationObject.State}");
                }
            }
            catch (CommunicationException ex)
            {
                Logger.Warn($"Communication failed while notifying {nickname}. Removing session. Error: {ex.Message}");
                SessionManager.RemoveSession(nickname);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error while notifying {nickname}. Error: {ex.Message}", ex);
            }
        }

        private static void NotifyRequestReceived(FriendRequestData request)
        {
            TryNotifyCallback(request.TargetNickname, cb =>
            {
                cb.FriendRequestReceived(request);
            });
        }
    }
}