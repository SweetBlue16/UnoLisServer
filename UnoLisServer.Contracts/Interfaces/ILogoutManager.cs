using System.ServiceModel;
using UnoLisServer.Common.Models;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(ILogoutCallback))]
    public interface ILogoutManager
    {
        [OperationContract(IsOneWay = true)]
        void Logout(string nickname);
    }

    [ServiceContract]
    public interface ILogoutCallback
    {
        [OperationContract(IsOneWay = true)]
        void LogoutResponse(ServiceResponse<object> response);
    }
}
