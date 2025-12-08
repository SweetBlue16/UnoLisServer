using System;
using System.Collections.Generic;
using System.Linq;
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
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[WCF] Communication error registering: {commEx.Message}");
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Warn($"[WCF] Timeout registering: {timeoutEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error registering player", ex);
            }
        }

        public void SendMessage(ChatMessageData message)
        {
            if (message == null)
            {
                return;
            }

            try
            {
                ChatValidator.ValidateMessage(message);
                string channel = message.ChannelId ?? "General";
                _sessionHelper.AddToHistory(channel, message);

                BroadcastToClients(message);
            }
            catch (ValidationException valEx)
            {
                Logger.Warn($"[CHAT] Message validation failed: {valEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Error orchestrating SendMessage", ex);
            }
        }

        private void BroadcastToClients(ChatMessageData message)
        {
            var activeClients = _sessionHelper.GetActiveClients();

            if (activeClients == null)
            {
                return;
            }

            var clientsSnapshot = activeClients.ToList();

            foreach (var client in clientsSnapshot)
            {
                try
                {
                    if (client is ICommunicationObject commObj && commObj.State == CommunicationState.Opened)
                    {
                        client.MessageReceived(message);
                    }
                }
                catch (CommunicationException)
                {
                    Logger.Warn($"[CHAT] Delivery failed: Client disconnected.");
                }
                catch (TimeoutException)
                {
                    Logger.Warn($"[CHAT] Delivery failed: Timeout.");
                }
                catch (ObjectDisposedException)
                {
                    Logger.Warn($"[CHAT] Delivery failed: Object disposed.");
                }
                catch (Exception ex)
                {
                    Logger.Warn($"[CHAT] Unexpected delivery error: {ex.Message}");
                }
            }
        }

        public void GetChatHistory(string channelId)
        {
            string targetChannel = channelId ?? "General";

            try
            {
                var callback = _callbackProvider.GetCallback();
                if (callback == null)
                {
                    Logger.Warn($"[CHAT] Cannot send history. Callback channel is null.");
                    return;
                }

                var historyList = _sessionHelper.GetHistory(targetChannel);
                var historyArray = historyList?.ToArray() ?? new ChatMessageData[0];

                callback.ChatHistoryReceived(historyArray);
            }
            catch (CommunicationException commEx)
            {
                Logger.Warn($"[WCF] Communication error sending history: {commEx.Message}");
            }
            catch (TimeoutException timeoutEx)
            {
                Logger.Warn($"[WCF] Timeout sending history: {timeoutEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[CRITICAL] Unexpected error retrieving chat history", ex);
            }
        }
    }
}