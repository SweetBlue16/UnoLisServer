using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IProfileViewCallback), SessionMode = SessionMode.Required)]
    public interface IProfileViewManager
    {
        [OperationContract(IsOneWay = true)]
        void GetProfileData(string nickname);
    }

    [ServiceContract]
    public interface IProfileViewCallback : ISessionCallback
    {
        [OperationContract (IsOneWay = true)]
        void ProfileDataReceived(ServiceResponse<ProfileData> response);
    }
}
