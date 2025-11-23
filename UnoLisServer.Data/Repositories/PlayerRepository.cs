using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Data;
using UnoLisServer.Data.RepositoryInterfaces;

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
    }
}
