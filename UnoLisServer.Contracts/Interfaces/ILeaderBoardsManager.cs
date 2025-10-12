using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(ILeaderboardsCallback), SessionMode = SessionMode.Required)]
    public interface ILeaderboardsManager
    {
        [OperationContract(IsOneWay = true)]
        void GetLeaderboard();

        [OperationContract(IsOneWay = true)]
        void GetPlayerRank(string nickname);
    }

    [ServiceContract]
    public interface ILeaderboardsCallback : ISessionCallback
    {
        [OperationContract]
        void LeaderboardReceived(List<LeaderboardEntry> entries);

        [OperationContract]
        void PlayerRankReceived(LeaderboardEntry entry);
    }
}
