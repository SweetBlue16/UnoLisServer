using System;
using System.Linq;
using UnoLisServer.Data.RepositoryInterfaces;

namespace UnoLisServer.Data.Repositories
{
    /// <summary>
    /// Provides methods for managing sanctions in the data store, including adding new sanctions and retrieving active
    /// sanctions for players.
    /// </summary>
    public class SanctionRepository : ISanctionRepository
    {
        private readonly Func<UNOContext> _contextFactory;

        public SanctionRepository() : this(() => new UNOContext())
        {
        }

        public SanctionRepository(Func<UNOContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void AddSanction(Sanction sanction)
        {
            using (var context = _contextFactory())
            {
                context.Sanction.Add(sanction);
                context.SaveChanges();
            }
        }

        public Sanction GetActiveSanction(int idPlayer)
        {
            using (var context = _contextFactory())
            {
                return context.Sanction
                    .Where(s => s.Player_idPlayer == idPlayer && s.sanctionEndDate > DateTime.UtcNow)
                    .OrderByDescending(s => s.sanctionEndDate)
                    .FirstOrDefault();
            }
        }
    }
}
