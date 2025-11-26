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
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            try
            {
                var callback = _callbackProvider.GetCallback();
                _sessionHelper.AddClient(nickname, callback);

                Logger.Log($"[CHAT] Jugador registrado: {nickname}");
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[CHAT] Error de comunicación al registrar a {nickname}: {commEx.Message}");
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Warn($"[CHAT] Timeout al registrar a {nickname}: {timeoutEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[CHAT] Error crítico al registrar jugador {nickname}", ex);
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

                BroadcastToClients(message);
            }
            catch (ValidationException valEx)
            {
                Logger.Warn($"[CHAT] Validación fallida mensaje de {message?.Nickname}: {valEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[CHAT] Error crítico en orquestación de SendMessage", ex);
            }
        }

        /// <summary>
        /// Auxiliar method to notify active clients in that a message has been sent
        /// </summary>
        private void BroadcastToClients(ChatMessageData message)
        {
            var activeClients = _sessionHelper.GetActiveClients();

            foreach (var client in activeClients)
            {
                try
                {
                    client.MessageReceived(message);
                }
                catch (CommunicationException)
                {
                    Logger.Warn($"[CHAT] No se pudo entregar mensaje: Cliente desconectado.");
                }
                catch (TimeoutException)
                {
                    Logger.Warn($"[CHAT] Timeout al entregar mensaje a un cliente.");
                }
                catch (ObjectDisposedException)
                {
                    Logger.Warn($"[CHAT] Intento de envío a un canal cerrado (ObjectDisposed).");
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[CHAT] Error inesperado enviando a cliente: {ex.Message}");
                }
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
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[CHAT] Error de comunicación al enviar historial (Cliente desconectado): {commEx.Message}");
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Warn($"[CHAT] Timeout al enviar historial al cliente: {timeoutEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[CHAT] Error crítico al recuperar historial", ex);
            }
        }
    }
}