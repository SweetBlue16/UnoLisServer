using System.Collections.Generic;
using System.ServiceModel;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IAvatarCallback), SessionMode = SessionMode.Required)]
    public interface IAvatarManager
    {
        [OperationContract(IsOneWay = true)]
        void GetPlayerAvatars(string nickname);
        [OperationContract(IsOneWay = true)]
        void SetPlayerAvatar(string nickname, int newAvatarId);
    }

    [ServiceContract]
    public interface IAvatarCallback : ISessionCallback
    {
        [OperationContract]
        void AvatarsDataReceived(ServiceResponse<List<PlayerAvatar>> response);
        [OperationContract]
        void AvatarUpdateResponse(ServiceResponse<object> response);
    }
}
