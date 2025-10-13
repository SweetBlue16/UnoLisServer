using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ChatManager : IChatManager
    {
        private readonly IChatCallback _callback;

        public ChatManager()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();
        }

        public void SendMessage(ChatMessageData message)
        {
            Console.WriteLine($"[{message.Nickname}] {message.Message}");
            _callback.MessageReceived(message);
        }

        public void GetChatHistory(string channelId)
        {
            var history = new List<ChatMessageData>
            {
                new ChatMessageData { Nickname = "Alice", Message = "¡Hola!" },
                new ChatMessageData { Nickname = "Bob", Message = "¡Bienvenido al juego!" }
            };

            _callback.ChatHistoryReceived(history);
        }
    }
}
