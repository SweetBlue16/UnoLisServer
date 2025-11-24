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
                    // 1. Obtener Entidades (Con Tracking activado para poder editar)
                    var player = await context.Player
                        .Include(p => p.Account)
                        .Include(p => p.SocialNetwork)
                        .FirstOrDefaultAsync(p => p.nickname == data.Nickname);

                    if (player == null) throw new Exception("PlayerNotFound"); // Manejaremos esto en el Manager

                    var account = player.Account.FirstOrDefault();
                    if (account == null) throw new Exception("AccountNotFound");

                    // 2. Actualizar Datos Básicos
                    player.fullName = data.FullName;
                    account.email = data.Email;

                    // 3. Actualizar Password (si aplica)
                    if (!string.IsNullOrWhiteSpace(data.Password))
                    {
                        account.password = PasswordHelper.HashPassword(data.Password);
                    }

                    // 4. Actualizar Redes Sociales (Lógica encapsulada)
                    UpdateOrAddNetwork(context, player, "Facebook", data.FacebookUrl);
                    UpdateOrAddNetwork(context, player, "Instagram", data.InstagramUrl);
                    UpdateOrAddNetwork(context, player, "TikTok", data.TikTokUrl);

                    // 5. Guardar y Commit
                    await context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw; // Re-lanzamos para que el Manager decida qué error mostrar
                }
            }
        }

        // Helper privado para redes sociales (ahora vive en el Repo, donde pertenece)
        private void UpdateOrAddNetwork(UNOContext context, Player player, string type, string url)
        {
            // Si la URL viene vacía, no hacemos nada (o podríamos borrarla si fuera la regla)
            if (string.IsNullOrWhiteSpace(url)) return;

            var existing = player.SocialNetwork.FirstOrDefault(sn => sn.tipoRedSocial == type);

            if (existing != null)
            {
                existing.linkRedSocial = url;
                // Al estar conectado al contexto, EF detecta el cambio automáticamente
            }
            else
            {
                var newNetwork = new SocialNetwork
                {
                    tipoRedSocial = type,
                    linkRedSocial = url,
                    Player_idPlayer = player.idPlayer
                    // No necesitamos asignar 'Player' objeto, con el ID basta o añadiéndolo a la lista
                };
                context.SocialNetwork.Add(newNetwork);
            }
        }
    }
}
