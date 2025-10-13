using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Data;
using UnoLisServer.Services;


namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class FriendsManager : IFriendsManager
    {
        private readonly IFriendsCallback _callback;

        public FriendsManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IFriendsCallback>();
        }

        public void GetFriendsList(string nickname)
        {
            // Por ahora sin conexión a BD, solo ejemplo de respuesta
            var mockList = new List<FriendData>
            {
                new FriendData { FriendNickname = "PlayerA", StatusMessage = "Accepted" },
                new FriendData { FriendNickname = "PlayerB", StatusMessage = "Pending" }
            };

            _callback.FriendsListReceived(mockList);
        }

        public void SendFriendRequest(FriendRequestData request)
        {
            _callback.FriendRequestResult(true, $"Solicitud enviada a {request.TargetNickname}.");
        }

        public void AcceptFriendRequest(FriendRequestData request)
        {
            _callback.FriendRequestResult(true, $"Solicitud aceptada de {request.RequesterNickname}.");
        }

        public void RemoveFriend(FriendRequestData request)
        {
            _callback.FriendRequestResult(true, $"Amigo {request.TargetNickname} eliminado.");
        }
    }
}
