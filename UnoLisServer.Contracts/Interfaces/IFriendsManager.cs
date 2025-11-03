using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IFriendsCallback), SessionMode = SessionMode.Required)]
    public interface IFriendsManager
    {
        [OperationContract(IsOneWay = true)]
        void GetFriendsList(string nickname);

        [OperationContract(IsOneWay = true)]
        void GetPendingRequests(string nickname);

        [OperationContract(IsOneWay = true)]
        void SendFriendRequest(FriendRequestData request);

        [OperationContract(IsOneWay = true)]
        void AcceptFriendRequest(FriendRequestData request);

        [OperationContract(IsOneWay = true)]
        void RejectFriendRequest(FriendRequestData request);

        [OperationContract(IsOneWay = true)]
        void RemoveFriend(FriendRequestData request);
    }

    [ServiceContract]
    public interface IFriendsCallback : ISessionCallback
    {
        [OperationContract]
        void FriendsListReceived(List<FriendData> friends);

        [OperationContract]
        void FriendRequestReceived(string fromNickname);

        [OperationContract]
        void PendingRequestsReceived(List<FriendRequestData> requests);

        [OperationContract]
        void FriendRequestResult(bool success, string message);

        [OperationContract]
        void FriendListUpdated(List<FriendData> updatedList);
    }
}
