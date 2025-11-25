using System;
using System.Collections.Generic;
using System.ServiceModel;
using UnoLisServer.Common.Enums;
using UnoLisServer.Common.Exceptions;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;
using UnoLisServer.Services.Helpers;
using UnoLisServer.Services.Providers;
using UnoLisServer.Services.Validators;

namespace UnoLisServer.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ChatManager : IChatManager
    {
        private readonly IChatSessionHelper _sessionHelper;
        private readonly IChatCallbackProvider _callbackProvider;

        public ChatManager() : this(ChatSessionHelper.Instance, new WcfChatCallbackProvider())
        {
        }

        public ChatManager(IChatSessionHelper sessionHelper, IChatCallbackProvider callbackProvider)
        {
            _sessionHelper = sessionHelper;
            _callbackProvider = callbackProvider;
        }

        public void RegisterPlayer(string nickname)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nickname)) return;

                var callback = _callbackProvider.GetCallback();

                _sessionHelper.AddClient(nickname, callback);

                Logger.Log($"[CHAT] Jugador registrado: {nickname}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[CHAT] Error al registrar jugador {nickname}", ex);
            }
        }

        public void SendMessage(ChatMessageData message)
        {
            try
            {
                ChatValidator.ValidateMessage(message);

                string channel = message.ChannelId ?? "General";
                _sessionHelper.AddToHistory(channel, message);

                Logger.Log($"[CHAT] Mensaje de {message.Nickname} en {channel}");

                var activeClients = _sessionHelper.GetActiveClients();
                var disconnectedClients = new List<IChatCallback>();

                foreach (var client in activeClients)
                {
                    try
                    {
                        client.MessageReceived(message);
                    }
                    catch (CommunicationException)
                    {
                        disconnectedClients.Add(client);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"[CHAT] Error enviando a un cliente: {ex.Message}");
                    }
                }
            }
            catch (ValidationException valEx)
            {
                Logger.Warn($"[CHAT] Validación fallida mensaje de {message?.Nickname}: {valEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[CHAT] Error crítico en SendMessage", ex);
            }
        }

        public void GetChatHistory(string channelId)
        {
            try
            {
                var history = _sessionHelper.GetHistory(channelId ?? "General");

                var callback = _callbackProvider.GetCallback();
                callback.ChatHistoryReceived(history.ToArray());

                Logger.Log($"[CHAT] Historial enviado a solicitante (Canal: {channelId})");
            }
            catch (Exception ex)
            {
                Logger.Error($"[CHAT] Error al recuperar historial", ex);
            }
        }
    }
}