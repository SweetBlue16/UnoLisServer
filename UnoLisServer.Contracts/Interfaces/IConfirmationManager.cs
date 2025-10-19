using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IConfirmationCallback), SessionMode = SessionMode.Required)]
    public interface IConfirmationManager
    {
        [OperationContract(IsOneWay = true)]
        void ConfirmCode(string email, string code);
    }

    [ServiceContract]
    public interface IConfirmationCallback : ISessionCallback
    {
        [OperationContract]
        void ConfirmationResponse(bool success);
    }
}
