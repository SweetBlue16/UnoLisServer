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
        Task SendMatchInvitationAsync(string email, string inviterNickname, string lobbyCode);
    }
    public class NotificationSender : INotificationSender
    {
        private readonly IEmailSender _sender;
        private static readonly Lazy<NotificationSender> _instance =
            new Lazy<NotificationSender>(() => new NotificationSender(new EmailSender()));

        public static INotificationSender Instance => _instance.Value;

        public NotificationSender(IEmailSender sender)
        {
            _sender = sender;
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

        public Task SendMatchInvitationAsync(string email, string inviterNickname, string lobbyCode)
        {
            var subject = "¡Invitación a Partida! - UNO LIS";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; text-align: center;'>
                    <h2>¡Has sido invitado a jugar!</h2>
                    <p>Tu amigo <strong>{inviterNickname}</strong> te ha invitado a una partida de UNO LIS.</p>
                    <br/>
                    <div style='background-color: #f0f0f0; padding: 20px; border-radius: 10px; display: inline-block;'>
                        <p style='margin: 0; font-size: 14px; color: #555;'>Código de la Sala:</p>
                        <h1 style='margin: 10px 0; color: #28a745; font-size: 40px; letter-spacing: 5px;'>{lobbyCode}</h1>
                    </div>
                    <br/><br/>
                    <p>Ingresa este código en la sección 'Join Match' del juego.</p>
                </body>
                </html>";

            return _sender.SendEmailAsync(email, subject, body);
        }
    }
}
