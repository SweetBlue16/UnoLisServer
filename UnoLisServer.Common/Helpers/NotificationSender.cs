using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Common.Helpers
{
    public interface INotificationSender
    {
        Task SendAccountVerificationEmailAsync(string email, string code);
        Task SendPasswordResetEmailAsync(string email, string code);
    }
    public class NotificationSender : INotificationSender
    {
        private readonly IEmailSender _sender;
        private static readonly Lazy<NotificationSender> _instance =
            new Lazy<NotificationSender>(() => new NotificationSender());

        public static INotificationSender Instance => _instance.Value;

        private NotificationSender()
        {
            _sender = new EmailSender();
        }

        public Task SendAccountVerificationEmailAsync(string email, string code)
        {
            throw new NotImplementedException();
            //var subject = "Código de Verificación - UNO LIS";
            //var body = $"<html><body>"
        }

        public Task SendPasswordResetEmailAsync(string email, string code)
        {
            throw new NotImplementedException();
        }
    }
}
