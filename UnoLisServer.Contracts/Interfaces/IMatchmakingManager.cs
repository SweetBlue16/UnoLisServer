using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    /// <summary>
    /// WCF Service Contract for creating and joining game matches.
    /// </summary>
    [ServiceContract]
    public interface IMatchmakingManager
    {
        [OperationContract]
        Task<CreateMatchResponse> CreateMatchAsync(MatchSettings settings);

        [OperationContract]
        Task<JoinMatchResponse> JoinMatchAsync(string lobbyCode, string nickname);
    }
}