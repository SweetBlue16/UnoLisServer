using System;
using System.ServiceModel;
using UnoLisServer.Common.Helpers;
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
            if (_callback == null)
            {
                Logger.Warn("[NOTIFICATIONS] Callback channel is null. Cannot send notification.");
                return;
            }

            try
            {
                _callback.NotificationReceived(data);
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[WCF] Failed to send notification to client. Connection might be closed. Error: {commEx.Message}");
            }
            catch (TimeoutException timeEx)
            {
                Logger.Warn($"[WCF] Timeout sending notification to client. Error: {timeEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error sending notification.", ex);

            }
        }
    }
}
