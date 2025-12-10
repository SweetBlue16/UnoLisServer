using System.ServiceModel;
using UnoLisServer.Common.Models;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IConfirmationCallback), SessionMode = SessionMode.Required)]
    public interface IConfirmationManager
    {
        [OperationContract(IsOneWay = true)]
        void ConfirmCode(string email, string code);

        [OperationContract(IsOneWay = true)]
        void ResendConfirmationCode(string email);
    }

    [ServiceContract]
    public interface IConfirmationCallback : ISessionCallback
    {
        [OperationContract (IsOneWay = true)]
        void ConfirmationResponse(ServiceResponse<object> response);

        [OperationContract (IsOneWay = true)]
        void ResendCodeResponse(ServiceResponse<object> response);
    }
}
