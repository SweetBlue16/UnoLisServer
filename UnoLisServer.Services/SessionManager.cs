using System;
using System.Collections.Generic;
using System.ServiceModel;
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
                    catch { }
                    Console.WriteLine($"[INFO] Sesión de '{nickname}' eliminada.");
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

