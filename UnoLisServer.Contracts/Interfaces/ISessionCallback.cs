using System;
using System.ServiceModel;

namespace UnoLisServer.Contracts.Interfaces
{
    /// <summary>
    /// Eventos transversales de sesión.
    /// Todas las interfaces de callback heredan de esta.
    /// </summary>
    [ServiceContract]
    public interface ISessionCallback
    {
        [OperationContract]
        void SessionExpired();

        [OperationContract]
        void PlayerDisconnected(string nickname);

        [OperationContract]
        void PlayerBanned(string reason, DateTime endDate);
    }
}
