using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Data.RepositoryInterfaces;

namespace UnoLisServer.Data.Repositories
{
    public class FriendRepository : IFriendRepository
    {
        private readonly Func<UNOContext> _contextFactory;

        public FriendRepository() : this(() => new UNOContext()) { }

        public FriendRepository(Func<UNOContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Player> GetPlayerByNicknameAsync(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return new Player();
            }

            using (var context = _contextFactory())
            {
                try
                {
                    return await context.Player
                        .AsNoTracking() 
                        .FirstOrDefaultAsync(p => p.nickname == nickname);
                }
                catch (SqlException sqlEx)
                {
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                    throw;
                }
                catch (EntityCommandExecutionException entityCmdEx)
                {
                    Logger.Error($"[EF-CRITICAL] Provider failed fetching player.", entityCmdEx);
                    throw new Exception("DataStore_Unavailable", entityCmdEx);
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Warn($"[DATA-TIMEOUT] Timeout fetching player.");
                    throw new Exception("Server_Busy", timeoutEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error fetching player.", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        public async Task<List<Player>> GetFriendsEntitiesAsync(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return new List<Player>();
            }

            using (var context = _contextFactory())
            {
                try
                {
                    var player = await context.Player
                        .AsNoTracking()
                        .FirstOrDefaultAsync(playerFriend => playerFriend.nickname == nickname);

                    if (player == null)
                    {
                        Logger.Warn($"[DATA] Player not found when fetching friends.");
                        return new List<Player>();
                    }

                    var friendships = await context.FriendList
                        .AsNoTracking()
                        .Include(friend => friend.Player)
                        .Include(friend => friend.Player1)
                        .Where(friendList => friendList.friendRequest == true &&
                                     (friendList.Player_idPlayer == player.idPlayer ||
                                     friendList.Player_idPlayer1 == player.idPlayer))
                        .ToListAsync();

                    var friends = new List<Player>();
                    foreach (var friend in friendships)
                    {
                        if (friend.Player_idPlayer == player.idPlayer)
                        {
                            friends.Add(friend.Player1);
                        }
                        else
                        {
                            friends.Add(friend.Player);
                        }
                    }

                    return friends;
                }
                catch (SqlException sqlEx)
                {
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                    throw;
                }
                catch (EntityCommandExecutionException cmdEx)
                {
                    Logger.Error($"[EF-CRITICAL] Provider failed fetching friends.", cmdEx);
                    throw new Exception("DataStore_Unavailable", cmdEx);
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Warn($"[DATA-TIMEOUT] Timeout fetching friends.");
                    throw new Exception("Server_Busy", timeEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error fetching friends.", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        public async Task<List<FriendList>> GetPendingRequestsEntitiesAsync(string targetNickname)
        {
            if (string.IsNullOrWhiteSpace(targetNickname))
            {
                return new List<FriendList>();
            }

            using (var context = _contextFactory())
            {
                try
                {
                    var targetId = await context.Player
                        .AsNoTracking()
                        .Where(player => player.nickname == targetNickname)
                        .Select(player => player.idPlayer)
                        .FirstOrDefaultAsync();

                    if (targetId == 0)
                    {
                        Logger.Warn($"[DATA] Target player not found fetching requests.");
                        return new List<FriendList>();
                    }

                    return await context.FriendList
                        .AsNoTracking()
                        .Include(friendList => friendList.Player)
                        .Where(friendList => friendList.Player_idPlayer1 == 
                        targetId && friendList.friendRequest == false)
                        .ToListAsync();
                }
                catch (SqlException sqlEx)
                {
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                    throw;
                }
                catch (EntityCommandExecutionException cmdEx)
                {
                    Logger.Error($"[EF-CRITICAL] Provider failed fetching requests.", cmdEx);
                    throw new Exception("DataStore_Unavailable", cmdEx);
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Warn($"[DATA-TIMEOUT] Timeout fetching requests.");
                    throw new Exception("Server_Busy", timeEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error fetching requests.", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        public async Task<FriendList> GetFriendshipEntryAsync(int userId1, int userId2)
        {
            using (var context = _contextFactory())
            {
                try
                {
                    return await context.FriendList
                        .AsNoTracking() 
                        .FirstOrDefaultAsync(friendList =>
                            (friendList.Player_idPlayer == userId1 && friendList.Player_idPlayer1 == userId2) ||
                            (friendList.Player_idPlayer1 == userId1 && friendList.Player_idPlayer == userId2));
                }
                catch (SqlException sqlEx)
                {
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                    throw;
                }
                catch (EntityCommandExecutionException cmdEx)
                {
                    Logger.Error($"[EF-CRITICAL] Provider failed checking friendship " +
                        $"between {userId1} and {userId2}.", cmdEx);
                    throw new Exception("DataStore_Unavailable", cmdEx);
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Warn($"[DATA-TIMEOUT] Timeout checking friendship between {userId1} and {userId2}.");
                    throw new Exception("Server_Busy", timeEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error checking friendship " +
                        $"between {userId1} and {userId2}.", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        public async Task<FriendList> CreateFriendRequestAsync(int requesterId, int targetId)
        {
            using (var context = _contextFactory())
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var newRequest = new FriendList
                    {
                        Player_idPlayer = requesterId,
                        Player_idPlayer1 = targetId,
                        friendRequest = false
                    };
                    context.FriendList.Add(newRequest);

                    await context.SaveChangesAsync();
                    transaction.Commit();

                    return newRequest;
                }
                catch (DbEntityValidationException valEx)
                {
                    transaction.Rollback();
                    var errorMessages = valEx.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => $"Property: {x.PropertyName} Error: {x.ErrorMessage}");
                    var fullError = string.Join("; ", errorMessages);

                    Logger.Error($"[DATA-VALIDATION] Entity validation failed creating request" +
                        $" {requesterId}->{targetId}: {fullError}", valEx); 
                    throw new Exception("Invalid_Data_Format", valEx);
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DATA-CONSTRAINT] Constraint violation creating request " +
                        $"{requesterId}->{targetId}.", dbEx);
                    throw new Exception("Data_Conflict", dbEx);
                }
                catch (SqlException sqlEx)
                {
                    transaction.Rollback();
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                    throw;
                }
                catch (TimeoutException timeEx)
                {
                    transaction.Rollback();
                    Logger.Warn($"[DATA-TIMEOUT] Transaction timed out creating request {requesterId}->{targetId}.");
                    throw new Exception("Server_Busy", timeEx);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"[CRITICAL] Unexpected error creating request {requesterId}->{targetId}", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        public async Task AcceptFriendRequestAsync(int friendshipId)
        {
            using (var context = _contextFactory())
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var request = await context.FriendList.FindAsync(friendshipId);

                    if (request != null)
                    {
                        request.friendRequest = true;

                        await context.SaveChangesAsync();
                        transaction.Commit();
                    }
                    else
                    {
                        Logger.Warn($"[DATA] Attempted to accept non-existent request ID {friendshipId}."); 
                        transaction.Rollback();
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DATA-CONSTRAINT] Update failed accepting request {friendshipId}.", dbEx);
                    throw new Exception("Data_Conflict", dbEx);
                }
                catch (SqlException sqlEx)
                {
                    transaction.Rollback();
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                }
                catch (TimeoutException timeEx)
                {
                    transaction.Rollback();
                    Logger.Warn($"[DATA-TIMEOUT] Transaction timed out accepting request {friendshipId}.");
                    throw new Exception("Server_Busy", timeEx);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"[CRITICAL] Unexpected error accepting request {friendshipId}", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        public async Task RemoveFriendshipEntryAsync(int friendshipId)
        {
            using (var context = _contextFactory())
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var entry = await context.FriendList.FindAsync(friendshipId);

                    if (entry != null)
                    {
                        context.FriendList.Remove(entry);

                        await context.SaveChangesAsync();
                        transaction.Commit();
                    }
                    else
                    {
                        Logger.Warn($"[DATA] Attempted to remove non-existent friendship ID {friendshipId}.");
                        transaction.Rollback();
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DATA-CONSTRAINT] Failed to remove friendship {friendshipId}. Constraint " +
                        $"violation.", dbEx);
                    throw new Exception("Data_Conflict", dbEx);
                }
                catch (SqlException sqlEx)
                {
                    transaction.Rollback();
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                    throw;
                }
                catch (TimeoutException timeEx)
                {
                    transaction.Rollback();
                    Logger.Warn($"[DATA-TIMEOUT] Transaction timed out removing friendship {friendshipId}.");
                    throw new Exception("Server_Busy", timeEx);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"[CRITICAL] Unexpected error removing friendship {friendshipId}", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }
    }
}