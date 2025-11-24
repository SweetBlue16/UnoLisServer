using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Data;
using UnoLisServer.Data.RepositoryInterfaces;
using UnoLisServer.Common.Helpers;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;

namespace UnoLisServer.Data.Repositories
{
    /// <summary>
    /// Repository class for accessing Player data
    /// </summary>
    public class PlayerRepository : IPlayerRepository
    {
        private readonly Func<UNOContext> _contextFactory;

        public PlayerRepository()
        {
            _contextFactory = () => new UNOContext();
        }

        public PlayerRepository(Func<UNOContext> contextFactory)
        {
            _contextFactory = contextFactory;
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
    }
}
