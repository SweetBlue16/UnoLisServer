using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IAuthCallback), SessionMode = SessionMode.Required)]
    public interface IAuthManager
    {
        [OperationContract(IsOneWay = true)]
        void Login(AuthCredentials credentials);

        [OperationContract(IsOneWay = true)]
        void Register(RegistrationData data);

        [OperationContract(IsOneWay = true)]
        void ConfirmCode(string email, string code);
    }

    [ServiceContract]
    public interface IAuthCallback : ISessionCallback
    {
        [OperationContract]
        void LoginResponse(bool success, string message);

        [OperationContract]
        void RegisterResponse(bool success, string message);

        [OperationContract]
        void ConfirmationResponse(bool success);
    }
}
