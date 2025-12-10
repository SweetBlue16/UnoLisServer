using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Common.Models;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract]
    public interface ILeaderboardsManager
    {
        [OperationContract]
        Task<ServiceResponse<List<LeaderboardEntry>>> GetGlobalLeaderboardAsync();
    }
}
