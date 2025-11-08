using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Common.Helpers;

namespace UnoLisServer.Services
{
    public class FriendsLogicManager : IFriendsLogicManager
    {
        public async Task<List<FriendData>> GetFriendsListAsync(string nickname)
        {
            using (var context = new UNOContext())
            {
                var player = await context.Player.FirstOrDefaultAsync(p => p.nickname == nickname);
                if (player == null)
                {
                    return new List<FriendData>();
                }

                var friends = await context.FriendList
                    .Where(fl => fl.friendRequest == true &&
                                 (fl.Player_idPlayer == player.idPlayer || fl.Player_idPlayer1 == player.idPlayer))
                    .ToListAsync();

                var friendDataList = new List<FriendData>();
                foreach (var fl in friends)
                {
                    var friendId = (fl.Player_idPlayer == player.idPlayer) ? fl.Player_idPlayer1 : fl.Player_idPlayer;
                    var friend = await context.Player.FindAsync(friendId);

                    if (friend != null)
                    {
                        friendDataList.Add(new FriendData
                        {
                            FriendNickname = friend.nickname,
                            IsOnline = SessionManager.IsOnline(friend.nickname),
                            StatusMessage = "Friend"
                        });
                    }
                }
                return friendDataList;
            }
        }

        public async Task<List<FriendRequestData>> GetPendingRequestsAsync(string nickname)
        {
            using (var context = new UNOContext())
            {
                var targetPlayer = await context.Player.FirstOrDefaultAsync(p => p.nickname == nickname);
                if (targetPlayer == null)
                {
                    return new List<FriendRequestData>();
                }

                var pendingRequests = await context.FriendList
                    .Where(fl => fl.Player_idPlayer1 == targetPlayer.idPlayer && fl.friendRequest == false)
                    .ToListAsync();

                var requestDataList = new List<FriendRequestData>();
                foreach (var req in pendingRequests)
                {
                    var requester = await context.Player.FindAsync(req.Player_idPlayer);
                    if (requester != null)
                    {
                        requestDataList.Add(new FriendRequestData
                        {
                            RequesterNickname = requester.nickname,
                            TargetNickname = nickname,
                            FriendListId = req.idFriendList
                        });
                    }
                }
                return requestDataList;
            }
        }

        public async Task<ValidationResultData> ValidateAndCheckExistingRequestAsync(
            string requesterNickname, string targetNickname)
        {
            using (var context = new UNOContext())
            {
                if (string.Equals(requesterNickname, targetNickname, StringComparison.OrdinalIgnoreCase))
                {
                    return new ValidationResultData { Result = FriendRequestResult.CannotAddSelf };
                }

                var requester = await context.Player.FirstOrDefaultAsync(p => p.nickname == requesterNickname);
                var target = await context.Player.FirstOrDefaultAsync(p => p.nickname == targetNickname);

                if (requester == null || target == null)
                {
                    return new ValidationResultData { Result = FriendRequestResult.UserNotFound };
                }

                var existingRequest = await context.FriendList.FirstOrDefaultAsync(fl =>
                    (fl.Player_idPlayer == requester.idPlayer && fl.Player_idPlayer1 == target.idPlayer) ||
                    (fl.Player_idPlayer1 == requester.idPlayer && fl.Player_idPlayer == target.idPlayer));

                if (existingRequest != null)
                {
                    if (existingRequest.friendRequest == true)
                        return new ValidationResultData { Result = FriendRequestResult.AlreadyFriends };

                    if (existingRequest.Player_idPlayer == target.idPlayer)
                    {
                        return new ValidationResultData { Result = FriendRequestResult.RequestAlreadyReceived };
                    }
                    return new ValidationResultData { Result = FriendRequestResult.RequestAlreadySent };
                }

                return new ValidationResultData
                {
                    Result = FriendRequestResult.Success,
                    RequesterId = requester.idPlayer,
                    TargetId = target.idPlayer
                };
            }
        }

        public async Task CreatePendingRequestAsync(int requesterId, int targetId)
        {
            using (var context = new UNOContext())
            {
                var newRequest = new FriendList
                {
                    Player_idPlayer = requesterId,
                    Player_idPlayer1 = targetId,
                    friendRequest = false
                };
                context.FriendList.Add(newRequest);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> AcceptRequestAsync(FriendRequestData request)
        {
            using (var context = new UNOContext())
            {
                var target = await context.Player.FirstOrDefaultAsync(p => p.nickname == request.TargetNickname);
                var requester = await context.Player.FirstOrDefaultAsync(p => p.nickname == request.RequesterNickname);

                if (target == null || requester == null) return false;

                var pendingRequest = await context.FriendList.FirstOrDefaultAsync(fl =>
                    fl.Player_idPlayer == requester.idPlayer &&
                    fl.Player_idPlayer1 == target.idPlayer &&
                    fl.friendRequest == false);

                if (pendingRequest == null) return false;

                pendingRequest.friendRequest = true;
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> RejectPendingRequestAsync(string targetNickname, string requesterNickname)
        {
            using (var context = new UNOContext())
            {
                var target = await context.Player.FirstOrDefaultAsync(p => p.nickname == targetNickname);
                var requester = await context.Player.FirstOrDefaultAsync(p => p.nickname == requesterNickname);

                if (target == null || requester == null) return false;

                var pendingRequest = await context.FriendList.FirstOrDefaultAsync(fl =>
                    fl.Player_idPlayer == requester.idPlayer &&
                    fl.Player_idPlayer1 == target.idPlayer &&
                    fl.friendRequest == false);

                if (pendingRequest == null) return false;

                context.FriendList.Remove(pendingRequest);
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> RemoveConfirmedFriendshipAsync(string user1Nickname, string user2Nickname)
        {
            using (var context = new UNOContext())
            {
                var user1 = await context.Player.FirstOrDefaultAsync(p => p.nickname == user1Nickname);
                var user2 = await context.Player.FirstOrDefaultAsync(p => p.nickname == user2Nickname);

                if (user1 == null || user2 == null) return false;

                var friendship = await context.FriendList.FirstOrDefaultAsync(fl =>
                    fl.friendRequest == true &&
                    ((fl.Player_idPlayer == user1.idPlayer && fl.Player_idPlayer1 == user2.idPlayer) ||
                     (fl.Player_idPlayer1 == user1.idPlayer && fl.Player_idPlayer == user2.idPlayer)));

                if (friendship == null) return false;

                context.FriendList.Remove(friendship);
                await context.SaveChangesAsync();
                return true;
            }
        }
    }
}