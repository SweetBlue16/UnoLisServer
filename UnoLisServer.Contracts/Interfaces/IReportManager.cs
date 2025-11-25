using System.ServiceModel;
using UnoLisServer.Common.Models;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(IReportCallback), SessionMode = SessionMode.Required)]
    public interface IReportManager
    {
        [OperationContract(IsOneWay = true)]
        void ReportPlayer(ReportData reportData);
    }

    [ServiceContract]
    public interface IReportCallback : ISessionCallback
    {
        [OperationContract(IsOneWay = true)]
        void ReportPlayerResponse(ServiceResponse<object> response);

        [OperationContract(IsOneWay = true)]
        void OnPlayerKicked(ServiceResponse<object> response);
    }
}
