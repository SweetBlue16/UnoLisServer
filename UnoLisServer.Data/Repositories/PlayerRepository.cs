using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Data;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Data.Factories;

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
                return await context.Player
                    .AsNoTracking()
                    .Include(p => p.Account)
                    .Include(p => p.PlayerStatistics)
                    .Include(p => p.SocialNetwork)
                    .Include(p => p.AvatarsUnlocked.Select(au => au.Avatar))
                    .FirstOrDefaultAsync(p => p.nickname == nickname);
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

                    if (player == null) throw new Exception("PlayerNotFound");

                    var account = player.Account.FirstOrDefault();
                    if (account == null) throw new Exception("AccountNotFound");

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
                    throw new Exception($"Error de validación en base de datos: {entityEx.Message}", entityEx);
                }
                catch (DbUpdateException)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (Exception)
                {
                    transaction.Rollback();
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
                return await context.Player.AnyAsync(p => p.nickname == nickname);
            }
        }

        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            using (var context = _contextFactory())
            {
                return await context.Account.AnyAsync(a => a.email == email);
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
            var newPlayer = _playerFactory.CreateNewPlayer(pendingData.Nickname, pendingData.FullName, email, pendingData.HashedPassword);

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
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<List<PlayerAvatar>> GetPlayerAvatarsAsync(string nickname)
        {
            using (var context = _contextFactory())
            {
                var player = await context.Player
                    .Include(p => p.AvatarsUnlocked.Select(au => au.Avatar))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.nickname == nickname);

                if (player == null) return null;

                var avatarList = new List<PlayerAvatar>();

                foreach (var unlocked in player.AvatarsUnlocked)
                {
                    avatarList.Add(new PlayerAvatar
                    {
                        AvatarId = unlocked.Avatar.idAvatar,
                        AvatarName = unlocked.Avatar.avatarName,
                        Description = unlocked.Avatar.avatarDescription,
                        Rarity = unlocked.Avatar.avatarRarity,
                        IsUnlocked = true,
                        IsSelected = (player.SelectedAvatar_Avatar_idAvatar == unlocked.Avatar.idAvatar)
                    });
                }

                return avatarList;
            }
        }

        public async Task UpdateSelectedAvatarAsync(string nickname, int newAvatarId)
        {
            using (var context = _contextFactory())
            {
                var player = await context.Player.FirstOrDefaultAsync(p => p.nickname == nickname);
                if (player == null) throw new Exception("PlayerNotFound");

                player.SelectedAvatar_Avatar_idAvatar = newAvatarId;
                await context.SaveChangesAsync();
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
    }
}
