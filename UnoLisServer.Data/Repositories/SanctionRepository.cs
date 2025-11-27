using System;
using System.Linq;
using UnoLisServer.Data.RepositoryInterfaces;

namespace UnoLisServer.Data.Repositories
{
    public class SanctionRepository : ISanctionRepository
    {
        private readonly Func<UNOContext> _contextFactory;
        private readonly ISanctionRepository _sanctionRepository;

        public SanctionRepository() : this(() => new UNOContext(), new SanctionRepository())
        {
        }

        public SanctionRepository(Func<UNOContext> contextFactory, ISanctionRepository sanctionRepository)
        {
            _contextFactory = contextFactory;
            _sanctionRepository = sanctionRepository ?? new SanctionRepository();
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
                    .Include("Report")
                    .Where(s => s.Player_idPlayer == idPlayer && s.sanctionEndDate > DateTime.Now)
                    .OrderByDescending(s => s.sanctionEndDate)
                    .FirstOrDefault();
            }
        }
    }
}
