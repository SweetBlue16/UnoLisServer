using System.Collections.Generic;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    public interface IFriendsLogicManager
    {
        Task<ValidationResultData> ValidateAndCheckExistingRequestAsync(
            string requesterNickname, string targetNickname);

        Task CreatePendingRequestAsync(int requesterId, int targetId);

        Task<bool> AcceptRequestAsync(FriendRequestData request);

        Task<bool> RejectPendingRequestAsync(string targetNickname, string requesterNickname);

        Task<bool> RemoveConfirmedFriendshipAsync(string user1Nickname, string user2Nickname);

        Task<List<FriendData>> GetFriendsListAsync(string nickname);

        Task<List<FriendRequestData>> GetPendingRequestsAsync(string nickname);
    }
}