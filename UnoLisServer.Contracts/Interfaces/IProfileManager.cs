using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IProfileCallback), SessionMode = SessionMode.Required)]
    public interface IProfileManager
    {
        [OperationContract(IsOneWay = true)]
        void GetProfileData(string nickname);

        [OperationContract(IsOneWay = true)]
        void UpdateProfileData(ProfileData data);
    }

    [ServiceContract]
    public interface IProfileCallback : ISessionCallback
    {
        [OperationContract]
        void ProfileDataReceived(ProfileData data);

        [OperationContract]
        void ProfileUpdateResponse(bool success, string message);
    }
}
