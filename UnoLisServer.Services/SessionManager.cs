using System;
using System.Collections.Generic;
using System.ServiceModel;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services
{
    /// <summary>
    /// Class tht handles the current session
    /// </summary>
    public static class SessionManager
    {
        private static readonly Dictionary<string, ISessionCallback> _activeSessions =
        new Dictionary<string, ISessionCallback>();
        private static readonly object _lock = new object();

        public static void AddSession(string nickname, ISessionCallback callback)
        {
            if (string.IsNullOrWhiteSpace(nickname) || callback == null)
            {
                return;
            }

            lock (_lock)
            {
                if (_activeSessions.ContainsKey(nickname))
                {
                    Logger.Warn($"[SESSION] Duplicate login detected for Player. Closing old session.");
                    var oldCallback = _activeSessions[nickname];

                    _activeSessions.Remove(nickname);

                    CloseChannelSafe(oldCallback);
                }

                _activeSessions.Add(nickname, callback);
                Logger.Log($"[SESSION] User connected. Total sessions: {_activeSessions.Count}");

                if (callback is ICommunicationObject channel)
                {
                    channel.Closed += (s, e) => RemoveSession(nickname);
                    channel.Faulted += (s, e) => RemoveSession(nickname);
                }
            }
        }

        public static void RemoveSession(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return;
            }

            ISessionCallback callbackToRemove = null;

            lock (_lock)
            {
                if (_activeSessions.ContainsKey(nickname))
                {
                    callbackToRemove = _activeSessions[nickname];
                    _activeSessions.Remove(nickname);
                    Logger.Log($"[SESSION] User removed.");
                }
            }

            if (callbackToRemove != null)
            {
                CloseChannelSafe(callbackToRemove);
            }
        }

        public static ISessionCallback GetSession(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                throw new ArgumentNullException(nameof(nickname));
            }

            lock (_lock)
            {
                if (_activeSessions.ContainsKey(nickname))
                {
                    return _activeSessions[nickname];
                }
            }

            throw new KeyNotFoundException($"Session not found for user. Please check IsOnline() " +
                $"before calling GetSession().");
        }

        public static bool IsOnline(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return false;
            }

            lock (_lock)
            {
                return _activeSessions.ContainsKey(nickname);
            }
        }

        private static void CloseChannelSafe(ISessionCallback callback)
        {
            if (callback is ICommunicationObject channel)
            {
                try
                {
                    if (channel.State == CommunicationState.Opened || channel.State == CommunicationState.Opening)
                    {
                        channel.Close();
                    }
                    else
                    {
                        channel.Abort();
                    }
                }
                catch (CommunicationException)
                {
                    channel.Abort();
                }
                catch (TimeoutException)
                {
                    channel.Abort();
                }
                catch (Exception ex)
                {
                    Logger.Error("[SESSION] Unexpected error closing channel.", ex);
                    channel.Abort();
                }
            }
        }
    }
}

