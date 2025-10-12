using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;

namespace UnoLisServer.Contracts.Interfaces
{
    [ServiceContract(CallbackContract = typeof(INotificationsCallback), SessionMode = SessionMode.Required)]
    public interface INotificationsManager
    {
        [OperationContract(IsOneWay = true)]
        void SendNotification(NotificationData data);
    }

    [ServiceContract]
    public interface INotificationsCallback : ISessionCallback
    {
        [OperationContract]
        void NotificationReceived(NotificationData data);
    }
}
