using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Data;
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
            using (var context = _contextFactory())
            {
                return await context.Player.FirstOrDefaultAsync(p => p.nickname == nickname);
            }
        }

        public async Task<List<Player>> GetFriendsEntitiesAsync(string nickname)
        {
            using (var context = _contextFactory())
            {
                try
                {
                    var player = await context.Player
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.nickname == nickname);

                    if (player == null)
                    {
                        return new List<Player>();
                    }

                    var friendships = await context.FriendList
                        .AsNoTracking()
                        .Include(f => f.Player)
                        .Include(f => f.Player1) 
                        .Where(fl => fl.friendRequest == true &&
                                     (fl.Player_idPlayer == player.idPlayer || fl.Player_idPlayer1 == player.idPlayer))
                        .ToListAsync();

                    var friends = new List<Player>();
                    foreach (var f in friendships)
                    {
                        if (f.Player_idPlayer == player.idPlayer)
                        {
                            friends.Add(f.Player1);
                        }
                        else
                        {
                            friends.Add(f.Player);
                        }
                    }

                    return friends;
                }
                catch (SqlException sqlEx)
                {
                    Logger.Error($"[DB] Error de conexión/SQL al obtener amigos de {nickname}", sqlEx);
                    throw;
                }
                catch (EntityCommandExecutionException cmdEx)
                {
                    Logger.Error($"[DB] Error ejecutando comando EF para amigos de {nickname}", cmdEx);
                    throw;
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Error($"[DB] Timeout al obtener amigos de {nickname}", timeEx);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error($"[DB] Error general al obtener amigos de {nickname}", ex);
                    throw;
                }
            }
        }

        public async Task<List<FriendList>> GetPendingRequestsEntitiesAsync(string targetNickname)
        {
            using (var context = _contextFactory())
            {
                try
                {
                    var target = await context.Player
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.nickname == targetNickname);

                    if (target == null)
                    {
                        return new List<FriendList>();
                    }

                    return await context.FriendList
                        .AsNoTracking()
                        .Include(fl => fl.Player)
                        .Where(fl => fl.Player_idPlayer1 == target.idPlayer && fl.friendRequest == false)
                        .ToListAsync();
                }
                catch (SqlException sqlEx)
                {
                    Logger.Error($"[DB] Error SQL al obtener solicitudes de {targetNickname}", sqlEx);
                    throw;
                }
                catch (EntityCommandExecutionException cmdEx)
                {
                    Logger.Error($"[DB] Error EF al obtener solicitudes de {targetNickname}", cmdEx);
                    throw;
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Error($"[DB] Timeout al obtener solicitudes de {targetNickname}", timeEx);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error($"[DB] Error general al obtener solicitudes de {targetNickname}", ex);
                    throw;
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
                        .FirstOrDefaultAsync(fl =>
                            (fl.Player_idPlayer == userId1 && fl.Player_idPlayer1 == userId2) ||
                            (fl.Player_idPlayer1 == userId1 && fl.Player_idPlayer == userId2));
                }
                catch (SqlException sqlEx)
                {
                    Logger.Error($"[DB] Error SQL buscando relación entre IDs {userId1} y {userId2}", sqlEx);
                    throw;
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Error($"[DB] Timeout buscando relación entre IDs {userId1} y {userId2}", timeEx);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error($"[DB] Error general buscando relación entre IDs {userId1} y {userId2}", ex);
                    throw;
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
                        .Select(x => $"Propiedad: {x.PropertyName} Error: {x.ErrorMessage}");
                    var fullError = string.Join("; ", errorMessages);

                    Logger.Error($"[DB] Error de validación al crear solicitud {requesterId}->{targetId}: {fullError}", valEx);
                    throw new EntityException($"Error de validación de datos: {fullError}", valEx);
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error de actualización (FK/Unique) al crear solicitud {requesterId}->{targetId}", dbEx);
                    throw;
                }
                catch (SqlException sqlEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error SQL crítico al crear solicitud {requesterId}->{targetId}", sqlEx);
                    throw;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error general inesperado al crear solicitud", ex);
                    throw;
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
                        Logger.Warn($"[DB] Se intentó aceptar la solicitud {friendshipId} pero no existe.");
                        transaction.Rollback();
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error de actualización al aceptar solicitud {friendshipId}", dbEx);
                    throw;
                }
                catch (SqlException sqlEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error SQL al aceptar solicitud {friendshipId}", sqlEx);
                    throw;
                }
                catch (TimeoutException timeEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Timeout al aceptar solicitud {friendshipId}", timeEx);
                    throw;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error general al aceptar solicitud {friendshipId}", ex);
                    throw;
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
                        Logger.Warn($"[DB] Se intentó eliminar la relación {friendshipId} pero no existe.");
                        transaction.Rollback();
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error de restricción al eliminar relación {friendshipId}", dbEx);
                    throw;
                }
                catch (SqlException sqlEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error SQL al eliminar relación {friendshipId}", sqlEx);
                    throw;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error general al eliminar relación {friendshipId}", ex);
                    throw;
                }
            }
        }
    }
}