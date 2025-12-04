namespace UnoLisServer.Data.RepositoryInterfaces
{
    public interface IReportRepository
    {
        void SaveReport(Report report);
        bool HasRecentReport(int reporterId, int reportedId);
        int CountReports(int playerId);
    }
}
