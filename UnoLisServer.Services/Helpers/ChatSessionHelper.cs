using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.DTOs;
using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services.Helpers
{
    public interface IChatSessionHelper
    {
        void AddClient(string nickname, IChatCallback callback);
        void RemoveClient(string nickname);
        List<IChatCallback> GetActiveClients();
        void AddToHistory(string channelId, ChatMessageData message);
        List<ChatMessageData> GetHistory(string channelId);
    }

    /// <summary>
    /// Singleton thread-safe que maneja las sesiones activas en memoria y un historial volátil.
    /// </summary>
    public class ChatSessionHelper : IChatSessionHelper
    {
        private static readonly Lazy<ChatSessionHelper> _instance =
            new Lazy<ChatSessionHelper>(() => new ChatSessionHelper());

        public static ChatSessionHelper Instance => _instance.Value;

        private readonly Dictionary<string, IChatCallback> _connectedClients = new Dictionary<string, IChatCallback>();
        private readonly Dictionary<string, List<ChatMessageData>> _chatHistory = new Dictionary<string, List<ChatMessageData>>();

        private readonly object _syncLock = new object();
        private const int MAX_HISTORY_MESSAGES = 50;

        private ChatSessionHelper() { }

        public void AddClient(string nickname, IChatCallback callback)
        {
            lock (_syncLock)
            {
                if (_connectedClients.ContainsKey(nickname))
                {
                    _connectedClients[nickname] = callback;
                    Logger.Log($"[CHAT-SESSION] Usuario {nickname} reconectado/actualizado.");
                }
                else
                {
                    _connectedClients.Add(nickname, callback);
                    Logger.Log($"[CHAT-SESSION] Usuario {nickname} agregado. Total: {_connectedClients.Count}");
                }
            }
        }

        public void RemoveClient(string nickname)
        {
            lock (_syncLock)
            {
                if (_connectedClients.ContainsKey(nickname))
                {
                    _connectedClients.Remove(nickname);
                    Logger.Log($"[CHAT-SESSION] Usuario {nickname} eliminado.");
                }
            }
        }

        public List<IChatCallback> GetActiveClients()
        {
            lock (_syncLock)
            {
                return _connectedClients.Values.ToList();
            }
        }

        public void AddToHistory(string channelId, ChatMessageData message)
        {
            lock (_syncLock)
            {
                if (!_chatHistory.ContainsKey(channelId))
                {
                    _chatHistory[channelId] = new List<ChatMessageData>();
                }

                var history = _chatHistory[channelId];
                history.Add(message);

                if (history.Count > MAX_HISTORY_MESSAGES)
                {
                    history.RemoveAt(0);
                }
            }
        }

        public List<ChatMessageData> GetHistory(string channelId)
        {
            lock (_syncLock)
            {
                if (_chatHistory.ContainsKey(channelId))
                {
                    return new List<ChatMessageData>(_chatHistory[channelId]);
                }
                return new List<ChatMessageData>();
            }
        }
    }
}