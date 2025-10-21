using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
        void LogoutResponse(bool success, string message);
    }
}
