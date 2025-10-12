using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Contracts;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class NotificationsManager : INotificationsManager
    {
        private readonly INotificationsCallback _callback;

        public NotificationsManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<INotificationsCallback>();
        }

        public void SendNotification(NotificationData data)
        {
            _callback.NotificationReceived(data);
        }
    }
}
