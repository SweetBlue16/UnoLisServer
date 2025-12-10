using System.ServiceModel;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IRegisterCallback), SessionMode = SessionMode.Required)]
    public interface IRegisterManager
    {
        [OperationContract(IsOneWay = true)]
        void Register(RegistrationData data);
    }

    [ServiceContract]
    public interface IRegisterCallback : ISessionCallback
    {
        [OperationContract (IsOneWay = true)]
        void RegisterResponse(ServiceResponse<object> response);
    }
}
