using System;
using System.Linq;
using UnoLisServer.Data.RepositoryInterfaces;

namespace UnoLisServer.Data.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private const int HoursBetweenReports = 24;
        private readonly Func<UNOContext> _contextFactory;

        public ReportRepository() : this(() => new UNOContext())
        {
        }

        public ReportRepository(Func<UNOContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public int CountReports(int playerId)
        {
            using (var context = _contextFactory())
            {
                return context.Report.Count(r => r.ReportedPlayer_idPlayer == playerId);
            }
        }

        public bool HasRecentReport(int reporterId, int reportedId)
        {
            using (var context = _contextFactory())
            {
                var cutoff = DateTime.UtcNow.AddHours(-HoursBetweenReports);
                return context.Report.Any(r =>
                    r.ReporterPlayer_idPlayer == reporterId &&
                    r.ReportedPlayer_idPlayer == reportedId &&
                    r.reportDate >= cutoff
                );
            }
        }

        public void SaveReport(Report report)
        {
            using (var context = _contextFactory())
            {
                context.Report.Add(report);
                context.SaveChanges();
            }
        }
    }
}
