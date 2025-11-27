namespace UnoLisServer.Data.RepositoryInterfaces
{
    public interface ISanctionRepository
    {
        Sanction GetActiveSanction(int idPlayer);
        void AddSanction(Sanction sanction);
    }
}
