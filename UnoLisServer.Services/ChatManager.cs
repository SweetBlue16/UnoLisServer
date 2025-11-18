using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services
{
    /// <summary>
    /// Duplex service for chat.
    /// Permite registro de jugadores, envío y recepción de mensajes.
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ChatManager : IChatManager
    {
        private readonly Dictionary<string, IChatCallback> _connectedClients =
            new Dictionary<string, IChatCallback>();

        private readonly object _syncLock = new object();

        public void RegisterPlayer(string nickname)
        {
            var callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();

            lock (_syncLock)
            {
                if (!_connectedClients.ContainsKey(nickname))
                {
                    _connectedClients.Add(nickname, callback);
                    Console.WriteLine($"✅ {nickname} se ha conectado al chat. ({_connectedClients.Count} usuarios activos)");
                }
                else
                {
                    _connectedClients[nickname] = callback;
                    Console.WriteLine($"♻️ {nickname} reconectado al chat.");
                }
            }
        }
        public void SendMessage(ChatMessageData message)
        {
            if (message == null)
                return;

            Console.WriteLine($"💬 [{message.Nickname}] {message.Message}");

            List<string> disconnectedClients = new List<string>();

            lock (_syncLock)
            {
                foreach (var client in _connectedClients.ToList())
                {
                    try
                    {
                        client.Value.MessageReceived(message);
                        Console.WriteLine($"✅ Enviado a {client.Key}");
                    }
                    catch (CommunicationException)
                    {
                        Console.WriteLine($"⚠️ Canal de {client.Key} se ha caído. Marcado para eliminación.");
                        disconnectedClients.Add(client.Key);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error enviando a {client.Key}: {ex.Message}");
                    }
                }

                foreach (var key in disconnectedClients)
                    _connectedClients.Remove(key);
            }
        }
        public void GetChatHistory(string channelId)
        {
            var history = new List<ChatMessageData>
            {
                new ChatMessageData { Nickname = "Alice", Message = "¡Hola!" },
                new ChatMessageData { Nickname = "Bob", Message = "¡Bienvenido al lobby!" }
            };

            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();
                callback.ChatHistoryReceived(history.ToArray());
                Console.WriteLine($"📜 Historial enviado ({history.Count} mensajes).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error al enviar historial: {ex.Message}");
            }
        }
    }
}
