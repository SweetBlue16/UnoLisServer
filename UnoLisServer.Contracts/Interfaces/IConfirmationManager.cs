using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
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
        [OperationContract]
        void ConfirmationResponse(ServiceResponse<object> response);

        [OperationContract]
        void ResendCodeResponse(ServiceResponse<object> response);
    }
}
