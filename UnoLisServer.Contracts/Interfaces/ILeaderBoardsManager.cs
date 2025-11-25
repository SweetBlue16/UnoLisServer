using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Common.Models;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract]
    public interface ILeaderboardsManager
    {
        [OperationContract]
        ServiceResponse<List<LeaderboardEntry>> GetGlobalLeaderboard();
    }
}
