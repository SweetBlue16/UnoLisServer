using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Data.RepositoryInterfaces
{
    public interface IFriendRepository
    {
        Task<List<Player>> GetFriendsEntitiesAsync(string nickname);
        Task<List<FriendList>> GetPendingRequestsEntitiesAsync(string targetNickname);
        Task<FriendList> GetFriendshipEntryAsync(int userId1, int userId2);
        Task<FriendList> CreateFriendRequestAsync(int requesterId, int targetId);
        Task AcceptFriendRequestAsync(int friendshipId);
        Task RemoveFriendshipEntryAsync(int friendshipId);
        Task<Player> GetPlayerByNicknameAsync(string nickname);
    }
}
