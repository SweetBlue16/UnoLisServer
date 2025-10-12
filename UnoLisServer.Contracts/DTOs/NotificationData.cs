using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnoLisServer.Contracts.DTOs
{
    public class NotificationData
    {
        public string Nickname { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
