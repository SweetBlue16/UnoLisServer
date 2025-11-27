using System;
using System.Collections.Generic;
using System.ServiceModel;
using UnoLisServer.Common.Helpers;
using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services
{
    public static class SessionManager
    {
        private static readonly Dictionary<string, ISessionCallback> _activeSessions =
        new Dictionary<string, ISessionCallback>();
        private static readonly object _lock = new object();

        public static void AddSession(string nickname, ISessionCallback callback)
        {
            lock (_lock)
            {
                if (!_activeSessions.ContainsKey(nickname))
                {
                    _activeSessions.Add(nickname, callback);
                    var channel = (ICommunicationObject)callback;
                    channel.Closed += (sender, args) => RemoveSession(nickname);
                    channel.Faulted += (sender, args) => RemoveSession(nickname);
                }
            }
        }

        public static void RemoveSession(string nickname)
        {
            lock (_lock)
            {
                if (_activeSessions.ContainsKey(nickname))
                {
                    var callback = _activeSessions[nickname];
                    _activeSessions.Remove(nickname);

                    var channel = (ICommunicationObject)callback;
                    try
                    {
                        channel.Closed -= (sender, args) => RemoveSession(nickname);
                        channel.Faulted -= (sender, args) => RemoveSession(nickname);
                    }
                    catch (CommunicationException commEx)
                    {
                        Logger.Log($"[WARNING] Events could not be unlinked '{nickname}': {commEx.Message}");
                    }
                    catch (TimeoutException timeoutEx)
                    {
                        Logger.Log($"[WARNING] Timeout reached unlinking events for '{nickname}': {timeoutEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"[ERROR] Unexpected errors unlinking events for '{nickname}': {ex.Message}");
                    }
                }
            }
        }

        public static ISessionCallback GetSession(string nickname)
        {
            lock (_lock)
            {
                return _activeSessions.ContainsKey(nickname) ? _activeSessions[nickname] : null;
            }
        }

        public static bool IsOnline(string nickname)
        {
            lock (_lock)
            {
                return _activeSessions.ContainsKey(nickname);
            }
        }
    }
}

