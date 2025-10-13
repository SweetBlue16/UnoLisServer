using System.Collections.Concurrent;
using UnoLisServer.Contracts;
using UnoLisServer.Contracts.Interfaces;

namespace UnoLisServer.Services
{
    public static class SessionManager
    {
        private static readonly ConcurrentDictionary<string, ISessionCallback> ActiveSessions =
        new ConcurrentDictionary<string, ISessionCallback>();


        public static void AddSession(string nickname, ISessionCallback callback)
        {
            ActiveSessions[nickname] = callback;
        }

        public static void RemoveSession(string nickname)
        {
            ActiveSessions.TryRemove(nickname, out _);
        }

        public static ISessionCallback GetSession(string nickname)
        {
            ActiveSessions.TryGetValue(nickname, out var callback);
            return callback;
        }

        public static bool IsOnline(string nickname)
        {
            return ActiveSessions.ContainsKey(nickname);
        }
    }
}

