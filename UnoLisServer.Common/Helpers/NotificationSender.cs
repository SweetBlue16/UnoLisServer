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
            var subject = "Código de Verificación - UNO LIS";
            var body = $"<html><body><h2>¡Gracias por registrarte en UNO-LIS!</h2>" +
                       $"<p>Tu código de verificación para completar el registro es:</p>" +
                       $"<h1 style='color: #007BFF;'>{code}</h1>" +
                       $"<p>Este código expirará en 5 minutos.</p></body></html>";
            return _sender.SendEmailAsync(email, subject, body);
        }

        public Task SendPasswordResetEmailAsync(string email, string code)
        {
            var subject = "Restablecimiento de Contraseña - UNO LIS";
            var body = $"<html><body><h2>Solicitud de cambio de contraseña.</h2>" +
                       $"<p>Tu código para restablecer la contraseña es:</p>" +
                       $"<h1 style='color: #FFC107;'>{code}</h1>" +
                       $"<p>Este código expirará en 5 minutos.</p></body></html>";
            return _sender.SendEmailAsync(email, subject, body);
        }
    }
}
