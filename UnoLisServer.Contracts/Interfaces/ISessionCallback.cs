﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
