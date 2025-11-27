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
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Data;
using UnoLisServer.Data.Factories;
using UnoLisServer.Data.RepositoryInterfaces;

namespace UnoLisServer.Data.Repositories
{
    /// <summary>
    /// Repository class for accessing Player data
    /// </summary>
    public class PlayerRepository : IPlayerRepository
    {
        private readonly Func<UNOContext> _contextFactory;
        private readonly IPlayerFactory _playerFactory;

        public PlayerRepository() : this(() => new UNOContext(), new PlayerFactory())
        {
        }

        public PlayerRepository(Func<UNOContext> contextFactory, IPlayerFactory playerFactory = null)
        {
            _contextFactory = contextFactory;
            _playerFactory = playerFactory ?? new PlayerFactory();
        }

        public async Task<Player> GetPlayerProfileByNicknameAsync(string nickname)
        {
            using (var context = _contextFactory())
            {
                try
                {
                    return await context.Player
                        .AsNoTracking()
                        .Include(p => p.Account)
                        .Include(p => p.PlayerStatistics)
                        .Include(p => p.SocialNetwork)
                        .Include(p => p.AvatarsUnlocked.Select(au => au.Avatar))
                        .FirstOrDefaultAsync(p => p.nickname == nickname);
                }
                catch (SqlException sqlEx)
                {
                    Logger.Error($"[DB] Error SQL al obtener perfil de {nickname}", sqlEx);
                    throw;
                }
                catch (EntityCommandExecutionException entityCmdEx)
                {
                    Logger.Error($"[DB] Error de ejecución de comando EF para {nickname}", entityCmdEx);
                    throw;
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Error($"[DB] Timeout al obtener perfil para {nickname}", timeoutEx);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error($"[DB] Error general al obtener perfil de {nickname}", ex);
                    throw;
                }
            }
        }

        public async Task UpdatePlayerProfileAsync(ProfileData data)
        {
            using (var context = _contextFactory())
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var player = await context.Player
                        .Include(p => p.Account)
                        .Include(p => p.SocialNetwork)
                        .FirstOrDefaultAsync(p => p.nickname == data.Nickname);

                    if (player == null)
                        throw new ValidationException(MessageCode.PlayerNotFound, $"Jugador '{data.Nickname}' no " +
                            $"encontrado.");

                    var account = player.Account.FirstOrDefault();
                    if (account == null)
                        throw new ValidationException(MessageCode.AccountNotVerified, "Cuenta asociada no encontrada.");

                    player.fullName = data.FullName;
                    account.email = data.Email;

                    if (!string.IsNullOrWhiteSpace(data.Password))
                    {
                        account.password = PasswordHelper.HashPassword(data.Password);
                    }

                    UpdateOrAddNetwork(context, player, "Facebook", data.FacebookUrl);
                    UpdateOrAddNetwork(context, player, "Instagram", data.InstagramUrl);
                    UpdateOrAddNetwork(context, player, "TikTok", data.TikTokUrl);

                    await context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (DbEntityValidationException entityEx)
                {
                    transaction.Rollback();
                    var errorMessages = entityEx.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);
                    string fullError = string.Join("; ", errorMessages);

                    Logger.Error($"[DB] Error de validación de entidad al actualizar {data.Nickname}: {fullError}", 
                        entityEx);
                    throw new EntityException($"Error de validación en base de datos: {fullError}", entityEx);
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error de actualización (SQL/FK) al actualizar {data.Nickname}", dbEx);
                    throw;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error inesperado al actualizar {data.Nickname}", ex);
                    throw;
                }
            }
        }

        private void UpdateOrAddNetwork(UNOContext context, Player player, string type, string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            var existing = player.SocialNetwork.FirstOrDefault(sn => sn.tipoRedSocial == type);

            if (existing != null)
            {
                existing.linkRedSocial = url;
            }
            else
            {
                var newNetwork = new SocialNetwork
                {
                    tipoRedSocial = type,
                    linkRedSocial = url,
                    Player_idPlayer = player.idPlayer
                };
                context.SocialNetwork.Add(newNetwork);
            }
        }

        public async Task<bool> IsNicknameTakenAsync(string nickname)
        {
            using (var context = _contextFactory())
            {
                try
                {
                    return await context.Player.AnyAsync(p => p.nickname == nickname);
                }
                catch (SqlException sqlEx)
                {
                    Logger.Error($"[DB] Error de conexión/SQL al verificar nickname '{nickname}'", sqlEx);
                    throw;
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Error($"[DB] Timeout agotado al verificar nickname '{nickname}'", timeoutEx);
                    throw;
                }
                catch (EntityCommandExecutionException entityCmdEx)
                {
                    Logger.Error($"[DB] Error interno de EF al verificar nickname '{nickname}'", entityCmdEx);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error($"[DB] Error general inesperado al verificar nickname '{nickname}'", ex);
                    throw;
                }
            }
        }

        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            using (var context = _contextFactory())
            {
                try
                {
                    return await context.Account.AnyAsync(a => a.email == email);
                }
                catch (SqlException sqlEx)
                {
                    Logger.Error($"[DB] Error de conexión/SQL al verificar email '{email}'", sqlEx);
                    throw;
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Error($"[DB] Timeout agotado al verificar email '{email}'", timeoutEx);
                    throw;
                }
                catch (EntityCommandExecutionException entityCmdEx)
                {
                    Logger.Error($"[DB] Error interno de EF al verificar email '{email}'", entityCmdEx);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error($"[DB] Error general inesperado al verificar email '{email}'", ex);
                    throw;
                }
            }
        }

        public async Task CreatePlayerAsync(RegistrationData data)
        {
            string passwordHash = PasswordHelper.HashPassword(data.Password);
            var newPlayer = _playerFactory.CreateNewPlayer(data.Nickname, data.FullName, data.Email, passwordHash);

            await SavePlayerGraphAsync(newPlayer);
        }

        public async Task CreatePlayerFromPendingAsync(string email, PendingRegistration pendingData)
        {
            var newPlayer = _playerFactory.CreateNewPlayer(pendingData.Nickname, pendingData.FullName, email, 
                pendingData.HashedPassword);

            await SavePlayerGraphAsync(newPlayer);
        }

        private async Task SavePlayerGraphAsync(Player playerEntity)
        {
            using (var context = _contextFactory())
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    context.Player.Add(playerEntity);
                    await context.SaveChangesAsync();

                    playerEntity.SelectedAvatar_Player_idPlayer = playerEntity.idPlayer;
                    playerEntity.SelectedAvatar_Avatar_idAvatar = 1;

                    context.Entry(playerEntity).State = EntityState.Modified;
                    await context.SaveChangesAsync();

                    transaction.Commit();
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error SQL al crear jugador {playerEntity.nickname}", dbEx);
                    throw;
                }
                catch (DbEntityValidationException valEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error validación entidad al crear jugador {playerEntity.nickname}", valEx);
                    throw;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"[DB] Error general al crear jugador {playerEntity.nickname}", ex);
                    throw;
                }
            }
        }

        public async Task<List<PlayerAvatar>> GetPlayerAvatarsAsync(string nickname)
        {
            using (var context = _contextFactory())
            {
                try
                {
                    var player = await context.Player
                        .AsNoTracking()
                        .Select(p => new { p.idPlayer, p.nickname, p.SelectedAvatar_Avatar_idAvatar })
                        .FirstOrDefaultAsync(p => p.nickname == nickname);

                    if (player == null) return null;

                    var unlockedAvatarIds = new HashSet<int>(await context.AvatarsUnlocked
                        .AsNoTracking()
                        .Where(au => au.Player_idPlayer == player.idPlayer)
                        .Select(au => au.Avatar_idAvatar)
                        .ToListAsync());

                    var allAvatars = await context.Avatar
                        .AsNoTracking()
                        .ToListAsync();

                    var resultList = new List<PlayerAvatar>();

                    foreach (var avatar in allAvatars)
                    {
                        bool isUnlocked = unlockedAvatarIds.Contains(avatar.idAvatar);

                        resultList.Add(new PlayerAvatar
                        {
                            AvatarId = avatar.idAvatar,
                            AvatarName = avatar.avatarName,
                            Description = avatar.avatarDescription,
                            Rarity = avatar.avatarRarity,
                            IsUnlocked = isUnlocked,
                            IsSelected = (player.SelectedAvatar_Avatar_idAvatar == avatar.idAvatar)
                        });
                    }

                    return resultList;
                }
                catch (SqlException sqlEx)
                {
                    Logger.Error($"[DB] Error crítico de SQL al obtener avatares para {nickname}", sqlEx);
                    throw;
                }
                catch (EntityCommandExecutionException entityCmdEx)
                {
                    Logger.Error($"[DB] Error de ejecución de comando EF para {nickname}", entityCmdEx);
                    throw;
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Error($"[DB] Timeout al obtener avatares para {nickname}", timeoutEx);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error($"[DB] Error general inesperado al obtener avatares para {nickname}", ex);
                    throw;
                }
            }
        }

        public async Task UpdateSelectedAvatarAsync(string nickname, int newAvatarId)
        {
            using (var context = _contextFactory())
            {
                try
                {
                    var player = await context.Player.FirstOrDefaultAsync(p => p.nickname == nickname);

                    if (player == null)
                        throw new ValidationException(MessageCode.PlayerNotFound, $"Jugador '{nickname}' no " +
                            $"encontrado para actualizar avatar.");

                    player.SelectedAvatar_Avatar_idAvatar = newAvatarId;
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    Logger.Error($"[DB] Error FK/SQL al cambiar avatar de {nickname} a {newAvatarId}", dbEx);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error($"[DB] Error inesperado al cambiar avatar de {nickname}", ex);
                    throw;
                }
            }
        }

        public List<PlayerStatistics> GetTopPlayersByGlobalScoreAsync(int topCount)
        {
            using (var context = _contextFactory())
            {
                var topPlayers = context.PlayerStatistics
                    .Include(ps => ps.Player)
                    .OrderByDescending(s => s.globalPoints)
                    .Take(topCount)
                    .ToList();
                return topPlayers;
            }
        }

        public async Task<Player> GetPlayerWithDetailsAsync(string nickname)
        {
            using (var context = _contextFactory())
            {
                try
                {
                    return await context.Player
                        .AsNoTracking()
                        .Include("Account")
                        .Include("AvatarsUnlocked.Avatar")
                        .FirstOrDefaultAsync(p => p.nickname == nickname);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[DB] Error obteniendo detalles completos de {nickname}", ex);
                    throw;
                }
            }
        }
    }
}