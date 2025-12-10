using System.ServiceModel;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IProfileEditCallback), SessionMode = SessionMode.Required)]
    public interface IProfileEditManager
    {
        [OperationContract(IsOneWay = true)]
        void UpdateProfileData(ProfileData data);

        [OperationContract(IsOneWay = true)]
        void RequestEmailChangeVerification(string nickname, string newEmail);
    }

    [ServiceContract]
    public interface IProfileEditCallback : ISessionCallback
    {
        [OperationContract (IsOneWay = true)]
        void ProfileUpdateResponse(ServiceResponse<ProfileData> response);

        [OperationContract (IsOneWay = true)]
        void EmailChangeVerificationCodeSentResponse(ServiceResponse<object> response);
    }
}
