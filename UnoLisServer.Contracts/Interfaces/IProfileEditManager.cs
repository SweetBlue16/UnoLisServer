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
    [ServiceContract(CallbackContract = typeof(IProfileEditCallback), SessionMode = SessionMode.Required)]
    public interface IProfileEditManager
    {
        [OperationContract(IsOneWay = true)]
        void UpdateProfileData(ProfileData data);
    }

    [ServiceContract]
    public interface IProfileEditCallback : ISessionCallback
    {
        [OperationContract]
        void ProfileUpdateResponse(ServiceResponse<object> response);
    }
}
