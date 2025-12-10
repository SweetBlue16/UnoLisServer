using System;
using System.Collections.Generic;
using UnoLisServer.Common.Models;

namespace UnoLisServer.Common.Helpers
{
    public interface IPendingRegistrationHelper
    {
        void StorePendingRegistration(string email, PendingRegistration data);
        PendingRegistration GetAndRemovePendingRegistration(string email);
    }

    public class PendingRegistrationHelper : IPendingRegistrationHelper
    {
        private static readonly Lazy<PendingRegistrationHelper> _instance =
            new Lazy<PendingRegistrationHelper>(() => new PendingRegistrationHelper());
        private readonly object _lockObject = new object();
        private readonly Dictionary<string, PendingRegistration> pendingRegistrations =
            new Dictionary<string, PendingRegistration>();

        public static IPendingRegistrationHelper Instance => _instance.Value;

        private PendingRegistrationHelper() { }

        public PendingRegistration GetAndRemovePendingRegistration(string email)
        {
            lock (_lockObject)
            {
                if (pendingRegistrations.TryGetValue(email, out PendingRegistration data))
                {
                    pendingRegistrations.Remove(email);
                    return data;
                }
                return new PendingRegistration();
            }
        }

        public void StorePendingRegistration(string email, PendingRegistration data)
        {
            lock (_lockObject)
            {
                pendingRegistrations[email] = data;
            }
        }
    }
}
