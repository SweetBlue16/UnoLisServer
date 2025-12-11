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
                        .Include(player => player.Account)
                        .Include(player => player.PlayerStatistics)
                        .Include(player => player.SocialNetwork)
                        .Include(player => player.AvatarsUnlocked.Select(avatarsUnlocked => avatarsUnlocked.Avatar))
                        .FirstOrDefaultAsync(player => player.nickname == nickname);
                }
                catch (SqlException sqlEx)
                {
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                    throw;
                }
                catch (EntityException entityEx)
                {
                    Logger.Error($"[EF-CRITICAL] Provider failed fetching profile.", entityEx);
                    throw new Exception("DataStore_Unavailable", entityEx);
                }
                catch (TimeoutException timeEx)
                {
                    Logger.Warn($"[DATA-TIMEOUT] Timed out fetching profile: {timeEx.Message}");
                    throw new Exception("Server_Busy", timeEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error fetching profile.", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        public async Task UpdatePlayerProfileAsync(ProfileData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Profile data cannot be null.");
            }

            string newPasswordHash = null;
            if (!string.IsNullOrWhiteSpace(data.Password))
            {
                newPasswordHash = PasswordHelper.HashPassword(data.Password);
            }

            using (var context = _contextFactory())
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var player = await context.Player
                        .Include(playerData => playerData.Account)
                        .Include(playerData => playerData.SocialNetwork)
                        .FirstOrDefaultAsync(playerData => playerData.nickname == data.Nickname);

                    if (player == null)
                    {
                        throw new ValidationException(MessageCode.PlayerNotFound, $"Player not found.");
                    }

                    var account = player.Account.FirstOrDefault();
                    if (account == null)
                    {
                        throw new ValidationException(MessageCode.AccountNotVerified, "Associated account not found.");
                    }

                    player.fullName = data.FullName;
                    account.email = data.Email;

                    if (newPasswordHash != null)
                    {
                        account.password = newPasswordHash;
                    }

                    UpdateOrAddNetwork(context, player, "Facebook", data.FacebookUrl);
                    UpdateOrAddNetwork(context, player, "Instagram", data.InstagramUrl);
                    UpdateOrAddNetwork(context, player, "TikTok", data.TikTokUrl);

                    await context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (ValidationException)
                {
                    transaction.Rollback();
                    throw; 
                }
                catch (DbEntityValidationException valEx)
                {
                    transaction.Rollback();
                    var errorMessages = valEx.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);
                    string fullError = string.Join("; ", errorMessages);

                    Logger.Error($"[DATA-VALIDATION] Entity validation failed updating: {fullError}", valEx);
                    throw new Exception("Invalid_Data_Format", valEx);
                }
                catch (SqlException sqlEx)
                {
                    transaction.Rollback();
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                }
                catch (EntityException entityEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[EF-CRITICAL] Provider failed during update.", entityEx);
                    throw new Exception("DataStore_Unavailable", entityEx);
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[DATA-CONSTRAINT] Constraint violation updating.", dbEx);
                    throw new Exception("Data_Conflict", dbEx);
                }
                catch (TimeoutException timeEx)
                {
                    transaction.Rollback();
                    Logger.Warn($"[DATA-TIMEOUT] Transaction timed out updating.");
                    throw new Exception("Server_Busy", timeEx);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"[CRITICAL] Unexpected error updating profile", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        public async Task<bool> IsNicknameTakenAsync(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return false;
            }

            using (var context = _contextFactory())
            {
                try
                {
                    return await context.Player.AnyAsync(p => p.nickname == nickname);
                }
                catch (SqlException sqlEx)
                {
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                    throw;
                }
                catch (EntityCommandExecutionException entityCmdEx)
                {
                    Logger.Error($"[EF-CRITICAL] Provider failed verifying nickname.", entityCmdEx);
                    throw new Exception("DataStore_Unavailable", entityCmdEx);
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Warn($"[DATA-TIMEOUT] Operation timed out verifying nickname.");
                    throw new Exception("Server_Busy", timeoutEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error verifying nickname.", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            using (var context = _contextFactory())
            {
                try
                {
                    return await context.Account.AnyAsync(a => a.email == email);
                }
                catch (SqlException sqlEx)
                {
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                    throw;
                }
                catch (EntityCommandExecutionException entityCmdEx)
                {
                    Logger.Error($"[EF-CRITICAL] Provider failed verifying email.", entityCmdEx);
                    throw new Exception("DataStore_Unavailable", entityCmdEx);
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Warn($"[DATA-TIMEOUT] Operation timed out verifying email.");
                    throw new Exception("Server_Busy", timeoutEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error verifying email.", ex);
                    throw new Exception("Server_Internal_Error", ex);
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

        public async Task<List<PlayerAvatar>> GetPlayerAvatarsAsync(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return new List<PlayerAvatar>();
            }

            using (var context = _contextFactory())
            {
                try
                {
                    var player = await context.Player
                        .AsNoTracking()
                        .Select(playerData => new { playerData.idPlayer, playerData.nickname, 
                            playerData.SelectedAvatar_Avatar_idAvatar })
                        .FirstOrDefaultAsync(playerData => playerData.nickname == nickname);

                    if (player == null)
                    {
                        throw new ValidationException(MessageCode.PlayerNotFound, $"Player not found.");
                    }

                    var unlockedAvatarIds = new HashSet<int>(await context.AvatarsUnlocked
                        .AsNoTracking()
                        .Where(avatarsUnlocked => avatarsUnlocked.Player_idPlayer == player.idPlayer)
                        .Select(avatarsUnlocked => avatarsUnlocked.Avatar_idAvatar)
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
                catch (ValidationException)
                {
                    throw;
                }
                catch (SqlException sqlEx)
                {
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                    throw;
                }
                catch (EntityCommandExecutionException entityCmdEx)
                {
                    Logger.Error($"[EF-CRITICAL] Provider failed fetching avatars.", entityCmdEx);
                    throw new Exception("DataStore_Unavailable", entityCmdEx);
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Warn($"[DATA-TIMEOUT] Operation timed out fetching avatars.");
                    throw new Exception("Server_Busy", timeoutEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error fetching avatars", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        public async Task UpdateSelectedAvatarAsync(string nickname, int newAvatarId)
        {
            using (var context = _contextFactory())
            {
                if (string.IsNullOrWhiteSpace(nickname))
                {
                    throw new ArgumentNullException(nameof(nickname));
                }

                try
                {
                    var player = await context.Player.FirstOrDefaultAsync(playerData => playerData.nickname == nickname);

                    if (player == null)
                    {
                        throw new ValidationException(MessageCode.PlayerNotFound, $"Jugador no " +
                            $"encontrado para actualizar avatar.");
                    }

                    player.SelectedAvatar_Avatar_idAvatar = newAvatarId;
                    await context.SaveChangesAsync();
                }
                catch (ValidationException)
                {
                    throw;
                }
                catch (DbUpdateException dbEx)
                {
                    Logger.Error($"[DATA-CONSTRAINT] Failed to update avatar. Target " +
                        $"AvatarID {newAvatarId} might be invalid.", dbEx);
                    throw new Exception("Data_Conflict", dbEx);
                }
                catch (SqlException sqlEx)
                {
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                }
                catch (EntityCommandExecutionException entityCmdEx)
                {
                    Logger.Error($"[EF-CRITICAL] Provider failed updating avatar.", entityCmdEx);
                    throw new Exception("DataStore_Unavailable", entityCmdEx);
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Warn($"[DATA-TIMEOUT] Operation timed out updating avatar.");
                    throw new Exception("Server_Busy", timeoutEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error updating avatar.", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        public async Task<List<PlayerStatistics>> GetTopPlayersByGlobalScoreAsync(int topCount)
        {
            if (topCount <= 0)
            {
                return new List<PlayerStatistics>();
            }

            using (var context = _contextFactory())
            {
                try
                {
                    return await context.PlayerStatistics
                        .AsNoTracking()
                        .Include(playerStatistics => playerStatistics.Player)
                        .OrderByDescending(statistics => statistics.globalPoints)
                        .Take(topCount)
                        .ToListAsync();
                }
                catch (SqlException sqlEx)
                {
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                    throw;
                }
                catch (EntityCommandExecutionException entityCmdEx)
                {
                    Logger.Error($"[EF-CRITICAL] Provider failed fetching top players.", entityCmdEx);
                    throw new Exception("DataStore_Unavailable", entityCmdEx);
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Warn($"[DATA-TIMEOUT] Operation timed out fetching top players.");
                    throw new Exception("Server_Busy", timeoutEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error fetching top players", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        public async Task<Player> GetPlayerWithDetailsAsync(string nickname)
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
                        .Include(player => player.Account)
                        .Include(player => player.AvatarsUnlocked.Select(avatarsUnlocked => avatarsUnlocked.Avatar)) 
                        .FirstOrDefaultAsync(player => player.nickname == nickname);
                }
                catch (SqlException sqlEx)
                {
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                    throw;
                }
                catch (EntityCommandExecutionException entityCmdEx)
                {
                    Logger.Error($"[EF-CRITICAL] Provider failed fetching details.", entityCmdEx);
                    throw new Exception("DataStore_Unavailable", entityCmdEx);
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Warn($"[DATA-TIMEOUT] Operation timed out fetching details.");
                    throw new Exception("Server_Busy", timeoutEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error fetching details.", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        public async Task UpdateMatchResultAsync(string nickname, bool isWinner, int pointsEarned)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            using (var context = _contextFactory())
            {
                try
                {
                    var player = await context.Player
                        .Include(playerData => playerData.PlayerStatistics)
                        .FirstOrDefaultAsync(playerData => playerData.nickname == nickname);

                    if (player == null)
                    {
                        Logger.Warn($"[DATA] Player not found. Match results skipped.");
                        return;
                    }

                    int coinsEarned = CalculateAndApplyCoins(player, pointsEarned);
                    var stats = EnsureStatisticsExist(context, player);

                    ApplyMatchLogicToStats(stats, isWinner, pointsEarned);
                    await context.SaveChangesAsync();

                    Logger.Log($"[DB] Stats updated: +{pointsEarned} pts, +{coinsEarned} coins.");
                }
                catch (SqlException sqlEx)
                {
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                }
                catch (DbUpdateException dbEx)
                {
                    Logger.Error($"[DATA-CONSTRAINT] Failed saving match results for {nickname}.", dbEx);
                    throw new Exception("Data_Conflict", dbEx);
                }
                catch (EntityCommandExecutionException entityCmdEx)
                {
                    Logger.Error($"[EF-CRITICAL] Provider failed updating match result for {nickname}.", entityCmdEx);
                    throw new Exception("DataStore_Unavailable", entityCmdEx);
                }
                catch (TimeoutException timeoutEx)
                {
                    Logger.Warn($"[DATA-TIMEOUT] Operation timed out updating match result for {nickname}.");
                    throw new Exception("Server_Busy", timeoutEx);
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CRITICAL] Unexpected error updating match result for {nickname}", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        private void UpdateOrAddNetwork(UNOContext context, Player player, string type, string url)
        {
            if (context == null || player == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            var existing = player.SocialNetwork.FirstOrDefault(socialNetwork => socialNetwork.tipoRedSocial == type);

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

        private async Task SavePlayerGraphAsync(Player playerEntity)
        {
            if (playerEntity == null)
            {
                throw new ArgumentNullException(nameof(playerEntity));
            }

            using (var context = _contextFactory())
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    int defaultAvatar = 1;
                    context.Player.Add(playerEntity);
                    await context.SaveChangesAsync();

                    playerEntity.SelectedAvatar_Player_idPlayer = playerEntity.idPlayer;
                    playerEntity.SelectedAvatar_Avatar_idAvatar = defaultAvatar;

                    context.Entry(playerEntity).State = EntityState.Modified;
                    await context.SaveChangesAsync();

                    transaction.Commit();
                }
                catch (DbEntityValidationException valEx)
                {
                    transaction.Rollback();
                    var errorMessages = valEx.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);
                    string fullError = string.Join("; ", errorMessages);

                    Logger.Error($"[DATA-VALIDATION] Entity validation failed creating player: {fullError}", valEx);
                    throw new Exception("Invalid_Data_Format", valEx);
                }
                catch (SqlException sqlEx)
                {
                    transaction.Rollback();
                    SqlErrorHandler.HandleAndThrow(sqlEx);
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();

                    if (dbEx.InnerException?.InnerException is SqlException sqlInner)
                    {
                        SqlErrorHandler.HandleAndThrow(sqlInner);
                    }

                    Logger.Error($"[DATA-CONSTRAINT] Database update failed creating player.", dbEx);
                    throw new Exception("Data_Conflict", dbEx);
                }
                catch (EntityException entityEx)
                {
                    transaction.Rollback();
                    Logger.Error($"[EF-CRITICAL] Provider failed creating player.", entityEx);
                    throw new Exception("DataStore_Unavailable", entityEx);
                }
                catch (TimeoutException timeEx)
                {
                    transaction.Rollback();
                    Logger.Warn($"[DATA-TIMEOUT] Transaction timed out creating player.");
                    throw new Exception("Server_Busy", timeEx);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.Error($"[CRITICAL] Unexpected error creating player", ex);
                    throw new Exception("Server_Internal_Error", ex);
                }
            }
        }

        private static int CalculateAndApplyCoins(Player player, int pointsEarned)
        {
            double coinPercentage = 0.10;
            int coins = (int)(pointsEarned * coinPercentage);
            player.revoCoins += coins;

            return coins;
        }

        private PlayerStatistics EnsureStatisticsExist(UNOContext context, Player player)
        {
            var stats = player.PlayerStatistics.FirstOrDefault();

            if (stats == null)
            {
                stats = new PlayerStatistics
                {
                    Player_idPlayer = player.idPlayer,
                    matchesPlayed = 0,
                    wins = 0,
                    loses = 0,
                    globalPoints = 0,
                    streak = 0,
                    maxStreak = 0
                };
                context.PlayerStatistics.Add(stats);
            }
            return stats;
        }

        private static void ApplyMatchLogicToStats(PlayerStatistics stats, bool isWinner, int pointsEarned)
        {
            if (stats == null)
            {
                return;
            }

            stats.matchesPlayed = (stats.matchesPlayed ?? 0) + 1;
            stats.globalPoints = (stats.globalPoints ?? 0) + pointsEarned;

            if (isWinner)
            {
                stats.wins = (stats.wins ?? 0) + 1;
                stats.streak = (stats.streak ?? 0) + 1;

                if (stats.streak > (stats.maxStreak ?? 0))
                {
                    stats.maxStreak = stats.streak;
                }
            }
            else
            {
                stats.loses = (stats.loses ?? 0) + 1;
                stats.streak = 0; 
            }
        }
    }
}