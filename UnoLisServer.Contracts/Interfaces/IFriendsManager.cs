using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(
        CallbackContract = typeof(IFriendsCallback),
        SessionMode = SessionMode.Required)]
    public interface IFriendsManager
    {

        [OperationContract]
        Task<List<FriendData>> GetFriendsListAsync(string nickname);

        [OperationContract]
        Task<List<FriendRequestData>> GetPendingRequestsAsync(string nickname);

        [OperationContract]
        Task<FriendRequestResult> SendFriendRequestAsync(string requesterNickname, string targetNickname);

        [OperationContract]
        Task<bool> AcceptFriendRequestAsync(FriendRequestData request);

        [OperationContract]
        Task<bool> RejectFriendRequestAsync(FriendRequestData request);

        [OperationContract]
        Task<bool> RemoveFriendAsync(FriendRequestData request);

        [OperationContract(IsOneWay = true)]
        void SubscribeToFriendUpdates(string nickname);

        [OperationContract(IsOneWay = true)]
        void UnsubscribeFromFriendUpdates(string nickname);
    }

    [ServiceContract]
    public interface IFriendsCallback: ISessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void FriendsListReceived(List<FriendData> friends);

        [OperationContract(IsOneWay = true)]
        void FriendRequestReceived(FriendRequestData newRequest);

        [OperationContract(IsOneWay = true)]
        void PendingRequestsReceived(List<FriendRequestData> requests);

        [OperationContract(IsOneWay = true)]
        void FriendListUpdated(List<FriendData> updatedList);

        [OperationContract(IsOneWay = true)]
        void FriendActionNotification(string message, bool isSuccess);
    }
}